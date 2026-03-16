namespace SPBackend.Services.Commands;

public sealed class CommandDispatchResult
{
    public CommandDispatchResult(Guid commandId, bool acknowledged)
    {
        CommandId = commandId;
        Acknowledged = acknowledged;
    }

    public Guid CommandId { get; }
    public bool Acknowledged { get; }
}
