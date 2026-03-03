namespace SPBackend.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? LastError { get; set; }
}
