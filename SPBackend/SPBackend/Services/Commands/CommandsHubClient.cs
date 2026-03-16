using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using SPBackend.Requests.Commands.AddPolicy;
using SPBackend.Requests.Commands.AddSchedule;
using SPBackend.Requests.Commands.AddTimeout;
using SPBackend.Requests.Commands.EditPolicy;
using SPBackend.Requests.Commands.EditSchedule;
using SPBackend.Requests.Commands.SetPlug;
using SPBackend.Requests.Commands.SetPlugName;
using SPBackend.Requests.Commands.TogglePolicy;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Services.Outbox;

namespace SPBackend.Services.Commands;

public sealed class CommandsHubClient : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CommandInboxOptions _options;
    private readonly ILogger<CommandsHubClient> _logger;
    private readonly SemaphoreSlim _dispatchLock = new(1, 1);
    private HubConnection? _connection;

    public CommandsHubClient(
        IServiceScopeFactory scopeFactory,
        IOptions<CommandInboxOptions> options,
        ILogger<CommandsHubClient> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Command inbox listener disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.HubUrl))
        {
            _logger.LogWarning("Command inbox hub url is not configured.");
            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<CommandEnvelope>("command", command => HandleCommandAsync(command, stoppingToken));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _connection.StartAsync(stoppingToken);
                _logger.LogInformation("Connected to command hub {HubUrl}.", _options.HubUrl);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to command hub. Retrying...");
                var delay = TimeSpan.FromSeconds(Math.Max(1, _options.ReconnectDelaySeconds));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task HandleCommandAsync(CommandEnvelope command, CancellationToken stoppingToken)
    {
        if (_connection == null)
        {
            return;
        }

        await _dispatchLock.WaitAsync(stoppingToken);
        string ackPayload;
        try
        {
            _logger.LogInformation("Received command {CommandId} {Method} {Path}.", command.Id, command.HttpMethod, command.Path);
            var result = await DispatchCommandAsync(command, stoppingToken);
            ackPayload = BuildAckPayload(result.Success, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed handling command {CommandId} {Method} {Path}.", command.Id, command.HttpMethod, command.Path);
            ackPayload = BuildAckPayload(false, "Unhandled command failure.");
        }
        finally
        {
            _dispatchLock.Release();
        }

        try
        {
            await _connection.InvokeAsync("Acknowledge", command.Id, ackPayload, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed acknowledging command {CommandId}.", command.Id);
        }
    }

    private async Task<DispatchResult> DispatchCommandAsync(CommandEnvelope command, CancellationToken cancellationToken)
    {
        var method = command.HttpMethod?.ToUpperInvariant() ?? string.Empty;
        var path = NormalizePath(command.Path);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            switch (method)
            {
                case "PUT" when path == "/plugs/status/set":
                    await mediator.Send(DeserializePayload<SetPlugRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "PUT" when path == "/plugs/name/set":
                    await mediator.Send(DeserializePayload<SetPlugNameRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "POST" when path == "/plugs/timeout":
                    await mediator.Send(DeserializePayload<AddTimeoutRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "POST" when path == "/schedules":
                    await mediator.Send(DeserializePayload<AddScheduleRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "PUT" when path == "/schedules":
                    await mediator.Send(DeserializePayload<EditScheduleRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "PUT" when path == "/schedules/toggle":
                    await mediator.Send(DeserializePayload<ToggleScheduleRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "POST" when path == "/policy":
                    await mediator.Send(DeserializePayload<AddPolicyRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "PUT" when path == "/policy":
                    await mediator.Send(DeserializePayload<EditPolicyRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                case "PUT" when path == "/policy/toggle":
                    await mediator.Send(DeserializePayload<TogglePolicyRequest>(command.Payload), cancellationToken);
                    return DispatchResult.SuccessResult();
                default:
                    _logger.LogWarning("Unsupported command route {Method} {Path}.", method, path);
                    return DispatchResult.Failure("Unsupported route.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {CommandId} failed during execution.", command.Id);
            return DispatchResult.Failure("Command execution failed.");
        }
    }

    private static T DeserializePayload<T>(JsonElement? payload)
    {
        if (payload == null || payload.Value.ValueKind == JsonValueKind.Null || payload.Value.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException("Command payload is missing.");
        }

        if (payload.Value.ValueKind == JsonValueKind.String)
        {
            var raw = payload.Value.GetString() ?? string.Empty;
            var resultFromString = JsonSerializer.Deserialize<T>(raw, OutboxJson.SerializerOptions);
            if (resultFromString == null)
            {
                throw new InvalidOperationException("Command payload could not be deserialized.");
            }

            return resultFromString;
        }

        var result = JsonSerializer.Deserialize<T>(payload.Value.GetRawText(), OutboxJson.SerializerOptions);
        if (result == null)
        {
            throw new InvalidOperationException("Command payload could not be deserialized.");
        }

        return result;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
        {
            trimmed = trimmed[..queryIndex];
        }

        return trimmed.StartsWith('/')
            ? trimmed.ToLowerInvariant()
            : "/" + trimmed.ToLowerInvariant();
    }

    private static string BuildAckPayload(bool success, string? message)
    {
        var payload = new
        {
            status = success ? "ok" : "error",
            message = string.IsNullOrWhiteSpace(message) ? null : message
        };

        return JsonSerializer.Serialize(payload);
    }

    private readonly record struct DispatchResult(bool Success, string? Message)
    {
        public static DispatchResult SuccessResult() => new(true, null);
        public static DispatchResult Failure(string message) => new(false, message);
    }
}
