using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SPBackend.Data;
using SPBackend.Models;

namespace SPBackend.Services.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxOptions _options;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollDelay = TimeSpan.FromSeconds(Math.Max(1, _options.PollSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_options.Enabled)
                {
                    await ProcessBatchAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing failed.");
            }

            try
            {
                await Task.Delay(pollDelay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var localDb = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var remoteDb = scope.ServiceProvider.GetRequiredService<RemoteDbContext>();

        var now = DateTime.UtcNow;
        var retryThreshold = now.AddSeconds(-Math.Max(1, _options.RetrySeconds));

        var messages = await localDb.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .Where(x => x.AttemptCount < _options.MaxAttempts)
            .Where(x => x.LastAttemptAtUtc == null || x.LastAttemptAtUtc <= retryThreshold)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(Math.Max(1, _options.BatchSize))
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            message.AttemptCount += 1;
            message.LastAttemptAtUtc = now;
            message.LastError = null;

            try
            {
                await ApplyMessageAsync(message, remoteDb, cancellationToken);
                message.ProcessedAtUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.LastError = ex.Message;
                _logger.LogError(ex, "Failed processing outbox message {OutboxId}.", message.Id);
            }

            await localDb.SaveChangesAsync(cancellationToken);
            remoteDb.ChangeTracker.Clear();
        }
    }

    private static async Task ApplyMessageAsync(OutboxMessage message, RemoteDbContext remoteDb, CancellationToken cancellationToken)
    {
        var entityType = remoteDb.Model.FindEntityType(message.EntityType);
        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type {message.EntityType} not found in remote context.");
        }

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            message.Payload,
            OutboxJson.SerializerOptions);

        if (payload == null)
        {
            throw new InvalidOperationException("Outbox payload could not be deserialized.");
        }

        var key = entityType.FindPrimaryKey();
        if (key == null)
        {
            throw new InvalidOperationException($"Entity type {message.EntityType} has no primary key.");
        }

        var keyValues = new object[key.Properties.Count];
        for (var i = 0; i < key.Properties.Count; i++)
        {
            var keyProperty = key.Properties[i];
            if (!payload.TryGetValue(keyProperty.Name, out var keyValue))
            {
                throw new InvalidOperationException($"Primary key {keyProperty.Name} missing from payload.");
            }

            keyValues[i] = ConvertJsonValue(keyValue, keyProperty.ClrType);
        }

        var existing = await remoteDb.FindAsync(entityType.ClrType, keyValues, cancellationToken);

        switch (ParseOperation(message.Operation))
        {
            case OutboxOperation.Deleted:
                if (existing != null)
                {
                    remoteDb.Remove(existing);
                    await remoteDb.SaveChangesAsync(cancellationToken);
                }
                return;
            case OutboxOperation.Added:
            case OutboxOperation.Modified:
                var target = existing ?? Activator.CreateInstance(entityType.ClrType);
                if (target == null)
                {
                    throw new InvalidOperationException($"Could not construct entity {entityType.ClrType.FullName}.");
                }

                ApplyValues(entityType, target, payload);

                if (existing == null)
                {
                    remoteDb.Add(target);
                }
                else
                {
                    remoteDb.Entry(target).State = EntityState.Modified;
                }

                await remoteDb.SaveChangesAsync(cancellationToken);
                return;
            default:
                throw new InvalidOperationException($"Unsupported outbox operation {message.Operation}.");
        }
    }

    private static void ApplyValues(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, object target, Dictionary<string, JsonElement> payload)
    {
        foreach (var property in entityType.GetProperties())
        {
            if (!payload.TryGetValue(property.Name, out var element))
            {
                continue;
            }

            var value = ConvertJsonValue(element, property.ClrType);
            var propertyInfo = property.PropertyInfo;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(target, value);
            }
        }
    }

    private static object? ConvertJsonValue(JsonElement element, Type targetType)
    {
        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (effectiveType == typeof(string))
        {
            return element.GetString();
        }

        if (effectiveType == typeof(Guid))
        {
            return element.ValueKind == JsonValueKind.String ? Guid.Parse(element.GetString() ?? string.Empty) : element.GetGuid();
        }

        if (effectiveType.IsEnum)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return Enum.Parse(effectiveType, element.GetString() ?? string.Empty, true);
            }

            return Enum.ToObject(effectiveType, element.GetInt32());
        }

        return JsonSerializer.Deserialize(element.GetRawText(), effectiveType, OutboxJson.SerializerOptions);
    }

    private static OutboxOperation ParseOperation(string operation)
    {
        if (Enum.TryParse<OutboxOperation>(operation, true, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Invalid outbox operation {operation}.");
    }
}
