namespace SPBackend.Services.Outbox;

public sealed class OutboxOptions
{
    public bool Enabled { get; set; } = true;
    public int PollSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int RetrySeconds { get; set; } = 30;
    public int MaxAttempts { get; set; } = 20;
}
