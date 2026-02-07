using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using SPBackend.Data;
using SPBackend.Models;

namespace SPBackend.Services.Mqtt;

public class MqttService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _isSubscribed;

    private static readonly string[] SubscriptionTopics =
    [
        "home/plugs/consumptions"
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

        var isBatch = TryBuildBatchConsumptions(payload, out var batchConsumptions, out var batchPlugStates);
        Consumption? singleConsumption = null;
        var isSingle = false;

        if (!isBatch && TryBuildConsumption(topic, payload, out var parsedSingleConsumption))
        {
            singleConsumption = parsedSingleConsumption;
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
            await ApplyConsumptionsAsync(dbContext, batchConsumptions, batchPlugStates);
            Console.WriteLine($"Saved {batchConsumptions.Count} consumptions from batch on {topic}");
            return;
        }

        await ApplyConsumptionsAsync(dbContext, [singleConsumption!], new Dictionary<long, bool>());
        Console.WriteLine($"Saved consumption {singleConsumption.TotalEnergy} for plug {singleConsumption.PlugId} on {topic}");
    }

    private static async Task ApplyConsumptionsAsync(IAppDbContext dbContext, IReadOnlyList<Consumption> consumptions, IReadOnlyDictionary<long, bool> plugStates)
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
        }

        await dbContext.SaveChangesAsync();
    }

    private static bool TryBuildBatchConsumptions(string payload, out List<Consumption> consumptions, out Dictionary<long, bool> plugStates)
    {
        consumptions = new List<Consumption>();
        plugStates = new Dictionary<long, bool>();

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

                if (!TryGetLongProperty(plugElement, ["plugId", "plug_id", "id", "deviceId", "device_id"], out var plugId))
                {
                    continue;
                }

                if (!TryGetLongProperty(plugElement, ["consumption", "energy", "totalEnergy", "total_energy", "power"], out var totalEnergy))
                {
                    continue;
                }

                var consumption = new Consumption
                {
                    PlugId = plugId,
                    TotalEnergy = totalEnergy,
                    Time = timestamp ?? DateTime.UtcNow
                };

                consumptions.Add(consumption);

                if (TryGetBoolProperty(plugElement, ["isOn", "is_on", "on"], out var isOn))
                {
                    plugStates[plugId] = isOn;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return consumptions.Count > 0;
    }

    private static bool TryBuildConsumption(string topic, string payload, out Consumption consumption)
    {
        consumption = null!;
        long? plugId = null;
        long? totalEnergy = null;
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
                        if (TryGetLongProperty(root, ["plugId", "plug_id", "deviceId", "device_id", "id"], out var parsedPlugId))
                        {
                            plugId = parsedPlugId;
                        }

                        if (TryGetLongProperty(root, ["totalEnergy", "total_energy", "energy", "consumption", "power"], out var parsedEnergy))
                        {
                            totalEnergy = parsedEnergy;
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
            else if (TryParseLongFlexible(trimmed, out var parsedEnergy))
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

    private static bool TryGetLongProperty(JsonElement root, string[] names, out long value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (TryGetLongValue(prop.Value, out value))
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

    private static bool TryGetLongValue(JsonElement element, out long value)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.String && TryParseLongFlexible(element.GetString() ?? string.Empty, out value))
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

    private static bool TryParseLongFlexible(string value, out long result)
    {
        if (TryExtractLongFromText(value, out result))
        {
            return true;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
        {
            result = Convert.ToInt64(Math.Round(floatValue));
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryExtractLongFromText(string value, out long result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return false;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            result = 0;
            return false;
        }

        return long.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
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
