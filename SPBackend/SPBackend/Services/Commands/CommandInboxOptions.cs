namespace SPBackend.Services.Commands;

public sealed class CommandInboxOptions
{
    public bool Enabled { get; set; } = true;
    public string HubUrl { get; set; } = string.Empty;
    public int ReconnectDelaySeconds { get; set; } = 5;
}
