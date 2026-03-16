using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using SPBackend.Data;
using SPBackend.Hubs;
using SPBackend.Models;
using SPBackend.Services.Outbox;

namespace SPBackend.Services.Commands;

public sealed class CommandDispatcher
{
    private readonly IAppDbContext _dbContext;
    private readonly IHubContext<CommandsHub> _hubContext;
    private readonly CommandAckTracker _ackTracker;
    private readonly ILogger<CommandDispatcher> _logger;

    public CommandDispatcher(
        IAppDbContext dbContext,
        IHubContext<CommandsHub> hubContext,
        CommandAckTracker ackTracker,
        ILogger<CommandDispatcher> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _ackTracker = ackTracker;
        _logger = logger;
    }

    public async Task<CommandDispatchResult> DispatchAsync(
        string httpMethod,
        string path,
        object? payload,
        string? userId,
        bool waitForAck,
        TimeSpan ackTimeout,
        CancellationToken cancellationToken)
    {
        var serializedPayload = JsonSerializer.Serialize(payload ?? new { }, OutboxJson.SerializerOptions);

        var command = new CommandMessage
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            HttpMethod = httpMethod,
            Path = path,
            Payload = serializedPayload,
            UserId = userId
        };

        _dbContext.Commands.Add(command);
        await _dbContext.SaveChangesAsync(cancellationToken);

        Task? ackTask = null;
        CancellationTokenSource? timeoutCts = null;

        if (waitForAck)
        {
            timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(ackTimeout);
            ackTask = _ackTracker.WaitForAckAsync(command.Id, timeoutCts.Token);
        }

        var envelope = new CommandEnvelope
        {
            Id = command.Id,
            CreatedAtUtc = command.CreatedAtUtc,
            HttpMethod = command.HttpMethod,
            Path = command.Path,
            Payload = command.Payload,
            UserId = command.UserId
        };

        await _hubContext.Clients.All.SendAsync("command", envelope, cancellationToken);

        if (!waitForAck)
        {
            return new CommandDispatchResult(command.Id, false);
        }

        try
        {
            await ackTask!;
            return new CommandDispatchResult(command.Id, true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Command {CommandId} acknowledgment timed out.", command.Id);
            return new CommandDispatchResult(command.Id, false);
        }
        finally
        {
            timeoutCts?.Dispose();
        }
    }
}
