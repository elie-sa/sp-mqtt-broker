using Microsoft.AspNetCore.SignalR;
using SPBackend.Services.Commands;

namespace SPBackend.Hubs;

public sealed class CommandsHub : Hub
{
    private readonly CommandAckTracker _ackTracker;

    public CommandsHub(CommandAckTracker ackTracker)
    {
        _ackTracker = ackTracker;
    }

    public Task Acknowledge(Guid commandId, string? payload = null)
    {
        return _ackTracker.AcknowledgeAsync(commandId, payload, Context.ConnectionId, Context.ConnectionAborted);
    }
}
