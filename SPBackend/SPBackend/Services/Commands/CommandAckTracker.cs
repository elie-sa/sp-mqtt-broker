using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;

namespace SPBackend.Services.Commands;

public sealed class CommandAckTracker
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _pending = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CommandAckTracker> _logger;

    public CommandAckTracker(IServiceScopeFactory scopeFactory, ILogger<CommandAckTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task WaitForAckAsync(Guid commandId, CancellationToken cancellationToken)
    {
        var tcs = _pending.GetOrAdd(
            commandId,
            _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                if (_pending.TryRemove(commandId, out var pending))
                {
                    pending.TrySetCanceled(cancellationToken);
                }
            });
        }

        return tcs.Task;
    }

    public async Task<bool> AcknowledgeAsync(Guid commandId, string? payload, string? acknowledgedBy, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var command = await db.Commands.FirstOrDefaultAsync(x => x.Id == commandId, cancellationToken);
            if (command == null)
            {
                _logger.LogWarning("Command {CommandId} not found while acknowledging.", commandId);
                return false;
            }

            if (command.AcknowledgedAtUtc == null)
            {
                command.AcknowledgedAtUtc = DateTime.UtcNow;
                command.AcknowledgedBy = acknowledgedBy;
                command.AcknowledgePayload = payload;
                await db.SaveChangesAsync(cancellationToken);
            }

            if (_pending.TryRemove(commandId, out var pending))
            {
                pending.TrySetResult(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge command {CommandId}.", commandId);
            return false;
        }
    }
}
