namespace SPBackend.Services.Commands;

public sealed class CommandInboxOptions
{
    public int AckTimeoutSeconds { get; set; } = 15;
}
