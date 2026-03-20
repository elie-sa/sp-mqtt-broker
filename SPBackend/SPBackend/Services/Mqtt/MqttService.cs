using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using SPBackend.Data;
using SPBackend.Models;

namespace SPBackend.Services.Mqtt;

public class MqttService : IMqttService
{
    private const double MinimumConsumptionThreshold = 5;
    private const string PowerSourceTopic = "home/powersource";
    private const string MainsConsumptionTopic = "home/mains/consumption";
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _isSubscribed;
    private long? _currentPowerSourceId;

    private static readonly string[] SubscriptionTopics =
    [
        "home/plugs/consumptions",
        PowerSourceTopic,
        MainsConsumptionTopic
    ];

    public MqttService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptions
        {
            ClientId = "backend-server",
            CleanSession = true,
            ChannelOptions = new MqttClientTcpOptions
            {
                Server = "localhost",
                Port = 1883
            }
        };

        _client.DisconnectedAsync += async e =>
        {
            await Task.Delay(5000);
            try { await ConnectAsync(); } catch { }
        };

        _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
    }

    public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(_options);
                _isSubscribed = false;
            }

            if (_isSubscribed)
            {
                return;
            }

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder();
            foreach (var topic in SubscriptionTopics)
            {
                subscribeOptions.WithTopicFilter(topic);
            }

            await _client.SubscribeAsync(subscribeOptions.Build());
            _isSubscribed = true;
    }

    public Task PublishAsync(string topic, string message)
    {
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        return _client.PublishAsync(msg);
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic ?? string.Empty;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

        if (string.Equals(topic, PowerSourceTopic, StringComparison.OrdinalIgnoreCase))
        {
            await HandlePowerSourceMessageAsync(payload);
            return;
        }

        if (string.Equals(topic, MainsConsumptionTopic, StringComparison.OrdinalIgnoreCase))
        {
            await HandleMainsConsumptionMessageAsync(payload);
            return;
        }

        var isBatch = TryBuildBatchConsumptions(payload, out var batchConsumptions, out var batchPlugStates, out var batchTemperatures);
        Consumption? singleConsumption = null;
        double? singleTemperature = null;
        var isSingle = false;

        if (!isBatch && TryBuildConsumption(topic, payload, out var parsedSingleConsumption, out var parsedSingleTemperature))
        {
            singleConsumption = parsedSingleConsumption;
            singleTemperature = parsedSingleTemperature;
            isSingle = true;
        }

        if (!isBatch && !isSingle)
        {
            Console.WriteLine($"Ignored {payload} on {topic}");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        if (isBatch)
        {
            if (batchConsumptions.Count == 0 && batchPlugStates.Count == 0)
            {
                return;
            }

            await ApplyConsumptionsAsync(dbContext, batchConsumptions, batchPlugStates, batchTemperatures);
            await EvaluateTemperaturePoliciesAsync(dbContext, batchTemperatures);
            Console.WriteLine($"Saved {batchConsumptions.Count} consumptions from batch on {topic}");
            return;
        }

        if (singleConsumption!.TotalEnergy < MinimumConsumptionThreshold)
        {
            var plugStates = new Dictionary<long, bool>
            {
                [singleConsumption.PlugId] = false
            };

            var tempUpdates = new Dictionary<long, double>();
            if (singleTemperature.HasValue)
            {
                tempUpdates[singleConsumption.PlugId] = singleTemperature.Value;
            }

            await ApplyConsumptionsAsync(dbContext, Array.Empty<Consumption>(), plugStates, tempUpdates);
            await EvaluateTemperaturePoliciesAsync(dbContext, tempUpdates);
            Console.WriteLine($"Marked plug {singleConsumption.PlugId} off due to low consumption on {topic}");
            return;
        }

        var temperatures = new Dictionary<long, double>();
        if (singleTemperature.HasValue)
        {
            temperatures[singleConsumption.PlugId] = singleTemperature.Value;
        }

        await ApplyConsumptionsAsync(dbContext, [singleConsumption], new Dictionary<long, bool>(), temperatures);
        await EvaluateTemperaturePoliciesAsync(dbContext, temperatures);
        Console.WriteLine($"Saved consumption {singleConsumption.TotalEnergy} for plug {singleConsumption.PlugId} on {topic}");
    }

    private async Task HandlePowerSourceMessageAsync(string payload)
    {
        if (!TryParsePowerSourceId(payload, out var powerSourceId))
        {
            Console.WriteLine($"Ignored power source payload {payload}");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var latestMainsLog = await dbContext.MainsLogs
            .OrderByDescending(x => x.Time)
            .FirstOrDefaultAsync();

        _currentPowerSourceId = powerSourceId;

        if (latestMainsLog == null)
        {
            Console.WriteLine("No mains log available to update power source.");
            return;
        }

        if (latestMainsLog.PowerSourceId == powerSourceId)
        {
            return;
        }

        latestMainsLog.PowerSourceId = powerSourceId;

        var policies = await dbContext.Policies
            .Include(p => p.PlugPolicies)
            .ThenInclude(pp => pp.Plug)
            .Where(p => p.IsActive && p.PowerSourceId == powerSourceId)
            .ToListAsync();

        if (policies.Count == 0)
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        var plugIds = policies
            .SelectMany(p => p.PlugPolicies)
            .Select(pp => pp.PlugId)
            .Distinct()
            .ToList();

        var recentConsumptions = await dbContext.RecentConsumptions
            .Where(rc => plugIds.Contains(rc.PlugId))
            .ToDictionaryAsync(rc => rc.PlugId);

        foreach (var policy in policies)
        {
            var requiresTemp = policy.TempGreaterThan != null || policy.TempLessThan != null;

            foreach (var plugPolicy in policy.PlugPolicies)
            {
                if (requiresTemp)
                {
                    if (!recentConsumptions.TryGetValue(plugPolicy.PlugId, out var recent) || recent.Temperature == null)
                    {
                        continue;
                    }

                    var temp = recent.Temperature.Value;
                    if (policy.TempGreaterThan != null && temp <= policy.TempGreaterThan.Value)
                    {
                        continue;
                    }

                    if (policy.TempLessThan != null && temp >= policy.TempLessThan.Value)
                    {
                        continue;
                    }
                }

                await PublishAsync($"home/plug/{plugPolicy.PlugId}", plugPolicy.SetStatus ? "ON" : "OFF");

                if (plugPolicy.Plug != null)
                {
                    plugPolicy.Plug.IsOn = plugPolicy.SetStatus;
                }
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task EvaluateTemperaturePoliciesAsync(IAppDbContext dbContext, IReadOnlyDictionary<long, double> temperatures)
    {
        if (temperatures.Count == 0)
        {
            return;
        }

        var currentPowerSourceId = await dbContext.MainsLogs
            .OrderByDescending(x => x.Time)
            .Select(x => (long?)x.PowerSourceId)
            .FirstOrDefaultAsync();

        var plugIds = temperatures.Keys.ToList();
        var plugPolicies = await dbContext.PlugPolicies
            .Include(pp => pp.Policy)
            .Include(pp => pp.Plug)
            .Where(pp =>
                plugIds.Contains(pp.PlugId) &&
                pp.Policy.IsActive &&
                (pp.Policy.TempGreaterThan != null || pp.Policy.TempLessThan != null))
            .ToListAsync();

        foreach (var plugPolicy in plugPolicies)
        {
            if (!temperatures.TryGetValue(plugPolicy.PlugId, out var temperature))
            {
                continue;
            }

            var policy = plugPolicy.Policy;
            if (policy.PowerSourceId != null && policy.PowerSourceId != currentPowerSourceId)
            {
                continue;
            }

            if (policy.TempGreaterThan != null && temperature <= policy.TempGreaterThan.Value)
            {
                continue;
            }

            if (policy.TempLessThan != null && temperature >= policy.TempLessThan.Value)
            {
                continue;
            }

            await PublishAsync($"home/plug/{plugPolicy.PlugId}", plugPolicy.SetStatus ? "ON" : "OFF");

            if (plugPolicy.Plug != null)
            {
                plugPolicy.Plug.IsOn = plugPolicy.SetStatus;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task HandleMainsConsumptionMessageAsync(string payload)
    {
        if (!TryBuildMainsConsumption(payload, out var voltage, out var amperage, out var time, out var payloadPowerSourceId))
        {
            Console.WriteLine($"Ignored mains consumption payload {payload}");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var latestMainsLog = await dbContext.MainsLogs
            .OrderByDescending(x => x.Time)
            .FirstOrDefaultAsync();

        var powerSourceId = payloadPowerSourceId ?? _currentPowerSourceId ?? latestMainsLog?.PowerSourceId;
        if (powerSourceId == null)
        {
            Console.WriteLine("No power source available for mains consumption payload.");
            return;
        }

        long? householdId = null;
        var powerSource = await dbContext.PowerSources.FirstOrDefaultAsync(x => x.Id == powerSourceId.Value);
        if (powerSource != null)
        {
            householdId = powerSource.HouseholdId;
        }
        else if (latestMainsLog != null)
        {
            householdId = latestMainsLog.HouseholdId;
        }

        if (householdId == null)
        {
            Console.WriteLine("No household available for mains consumption payload.");
            return;
        }

        var mainsLog = new MainsLog
        {
            Time = time,
            Voltage = voltage,
            Amperage = amperage,
            PowerSourceId = powerSourceId.Value,
            HouseholdId = householdId.Value
        };

        dbContext.MainsLogs.Add(mainsLog);
        await dbContext.SaveChangesAsync();
    }

    private static async Task ApplyConsumptionsAsync(IAppDbContext dbContext, IReadOnlyList<Consumption> consumptions, IReadOnlyDictionary<long, bool> plugStates, IReadOnlyDictionary<long, double> temperatures)
    {
        foreach (var plugState in plugStates)
        {
            var plug = await dbContext.Plugs.FindAsync(plugState.Key);
            if (plug == null)
            {
                continue;
            }

            plug.IsOn = plugState.Value;
        }

        foreach (var consumption in consumptions)
        {
            var dailyAggregate = await dbContext.Consumptions.FirstOrDefaultAsync(c =>
                c.PlugId == consumption.PlugId &&
                c.Time.Date == consumption.Time.Date &&
                c.Time.TimeOfDay == TimeSpan.Zero);

            if (dailyAggregate == null)
            {
                dailyAggregate = new Consumption
                {
                    PlugId = consumption.PlugId,
                    Time = consumption.Time.Date,
                    TotalEnergy = 0
                };
                dbContext.Consumptions.Add(dailyAggregate);
            }

            dailyAggregate.TotalEnergy += consumption.TotalEnergy;

            var recent = await dbContext.RecentConsumptions
                .FirstOrDefaultAsync(c => c.PlugId == consumption.PlugId);

            if (recent == null)
            {
                recent = new RecentConsumption
                {
                    PlugId = consumption.PlugId,
                    Time = consumption.Time,
                    TotalEnergy = consumption.TotalEnergy
                };
                dbContext.RecentConsumptions.Add(recent);
            }
            else
            {
                recent.Time = consumption.Time;
                recent.TotalEnergy = consumption.TotalEnergy;
            }

            if (temperatures.TryGetValue(consumption.PlugId, out var temperature))
            {
                recent.Temperature = temperature;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static bool TryBuildBatchConsumptions(string payload, out List<Consumption> consumptions, out Dictionary<long, bool> plugStates, out Dictionary<long, double> temperatures)
    {
        consumptions = new List<Consumption>();
        plugStates = new Dictionary<long, bool>();
        temperatures = new Dictionary<long, double>();

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var trimmed = payload.Trim();
        if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            DateTime? timestamp = null;
            if (TryGetDateTimeProperty(root, ["timestamp", "time", "ts"], out var parsedTime))
            {
                timestamp = parsedTime;
            }

            if (!root.TryGetProperty("plugs", out var plugsElement) || plugsElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var plugElement in plugsElement.EnumerateArray())
            {
                if (plugElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!TryGetDoubleProperty(plugElement, ["plugId", "plug_id", "id", "deviceId", "device_id"], out var plugId))
                {
                    continue;
                }

                if (!TryGetDoubleProperty(plugElement, ["consumption", "energy", "totalEnergy", "total_energy", "power"], out var totalEnergy))
                {
                    continue;
                }

                if (TryGetDoubleProperty(plugElement, ["temperature", "temp", "t"], out var parsedTemperature))
                {
                    temperatures[(long)plugId] = parsedTemperature;
                }

                if (totalEnergy < MinimumConsumptionThreshold)
                {
                    plugStates[(long)plugId] = false;
                    continue;
                }

                consumptions.Add(new Consumption
                {
                    PlugId = (long)plugId,
                    TotalEnergy = totalEnergy,
                    Time = timestamp ?? DateTime.UtcNow
                });

                if (TryGetBoolProperty(plugElement, ["isOn", "is_on", "on"], out var isOn))
                {
                    plugStates[(long)plugId] = isOn;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return consumptions.Count > 0 || plugStates.Count > 0;
    }

    private static bool TryBuildConsumption(string topic, string payload, out Consumption consumption, out double? temperature)
    {
        consumption = null!;
        long? plugId = null;
        double? totalEnergy = null;
        temperature = null;
        DateTime? time = null;

        if (!string.IsNullOrWhiteSpace(topic))
        {
            var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (long.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPlugId))
                {
                    plugId = parsedPlugId;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(payload))
        {
            var trimmed = payload.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
            {
                try
                {
                    using var document = JsonDocument.Parse(trimmed);
                    var root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (TryGetDoubleProperty(root, ["plugId", "plug_id", "deviceId", "device_id", "id"], out var parsedPlugId))
                        {
                            plugId = (long) parsedPlugId;
                        }

                        if (TryGetDoubleProperty(root, ["totalEnergy", "total_energy", "energy", "consumption", "power"], out var parsedEnergy))
                        {
                            totalEnergy = parsedEnergy;
                        }

                        if (TryGetDoubleProperty(root, ["temperature", "temp", "t"], out var parsedTemperature))
                        {
                            temperature = parsedTemperature;
                        }

                        if (TryGetDateTimeProperty(root, ["time", "timestamp", "ts"], out var parsedTime))
                        {
                            time = parsedTime;
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }
            else if (TryParseDoubleFlexible(trimmed, out var parsedEnergy))
            {
                totalEnergy = parsedEnergy;
            }
        }

        if (!plugId.HasValue || !totalEnergy.HasValue)
        {
            return false;
        }

        consumption = new Consumption
        {
            PlugId = plugId.Value,
            TotalEnergy = totalEnergy.Value,
            Time = time ?? DateTime.UtcNow
        };

        return true;
    }

    private static bool TryParsePowerSourceId(string payload, out long powerSourceId)
    {
        powerSourceId = 0;

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var trimmed = payload.Trim();

        if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object &&
                    TryGetDoubleProperty(root, ["powerSourceId", "power_source_id", "powerSource", "sourceId", "id"], out var parsedId))
                {
                    powerSourceId = (long)parsedId;
                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }

        if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            powerSourceId = id;
            return true;
        }

        if (TryParseDoubleFlexible(trimmed, out var parsedDouble))
        {
            powerSourceId = (long)parsedDouble;
            return true;
        }

        return false;
    }

    private static bool TryBuildMainsConsumption(string payload, out long voltage, out long amperage, out DateTime time, out long? powerSourceId)
    {
        voltage = 0;
        amperage = 0;
        time = DateTime.UtcNow;
        powerSourceId = null;

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var trimmed = payload.Trim();

        if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!TryGetDoubleProperty(root, ["voltage", "volt", "v"], out var parsedVoltage))
            {
                return false;
            }

            if (!TryGetDoubleProperty(root, ["amperage", "amp", "current", "a"], out var parsedAmperage))
            {
                return false;
            }

            voltage = ConvertToLong(parsedVoltage);
            amperage = ConvertToLong(parsedAmperage);

            if (TryGetDateTimeProperty(root, ["time", "timestamp", "ts"], out var parsedTime))
            {
                time = parsedTime;
            }

            if (TryGetDoubleProperty(root, ["powerSourceId", "power_source_id", "powerSource", "sourceId", "id"], out var parsedPowerSourceId))
            {
                powerSourceId = ConvertToLong(parsedPowerSourceId);
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static long ConvertToLong(double value)
    {
        return Convert.ToInt64(Math.Round(value, MidpointRounding.AwayFromZero));
    }

    private static bool TryGetBoolProperty(JsonElement root, string[] names, out bool value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (TryGetBoolValue(prop.Value, out value))
                {
                    return true;
                }
            }
        }

        value = false;
        return false;
    }

    private static bool TryGetBoolValue(JsonElement element, out bool value)
    {
        if (element.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var numeric))
        {
            value = numeric != 0;
            return true;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var raw = element.GetString();
            if (bool.TryParse(raw, out value))
            {
                return true;
            }

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                value = parsed != 0;
                return true;
            }
        }

        value = false;
        return false;
    }

    private static bool TryGetDoubleProperty(JsonElement root, string[] names, out double value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (TryGetDoubleValue(prop.Value, out value))
                {
                    return true;
                }
            }
        }

        value = 0;
        return false;
    }

    private static bool TryGetDateTimeProperty(JsonElement root, string[] names, out DateTime value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (TryGetDateTimeValue(prop.Value, out value))
                {
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool TryGetDoubleValue(JsonElement element, out double value)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.String && TryParseDoubleFlexible(element.GetString() ?? string.Empty, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryGetDateTimeValue(JsonElement element, out DateTime value)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(element.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value))
            {
                return true;
            }
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var epoch))
        {
            var fromEpoch = FromEpoch(epoch);
            if (fromEpoch.HasValue)
            {
                value = fromEpoch.Value;
                return true;
            }
        }

        if (element.ValueKind == JsonValueKind.String && long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var epochString))
        {
            var fromEpoch = FromEpoch(epochString);
            if (fromEpoch.HasValue)
            {
                value = fromEpoch.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool TryParseDoubleFlexible(string value, out double result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return false;
        }

        var trimmed = value.Trim();

        var cleaned = new string(trimmed.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

        return double.TryParse(
            cleaned,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result
        );
    }

    private static DateTime? FromEpoch(long epoch)
    {
        try
        {
            if (epoch > 9_999_999_999)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(epoch).UtcDateTime;
            }

            if (epoch > 0)
            {
                return DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        return null;
    }
}
