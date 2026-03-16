namespace SPBackend.Models;

public class CommandMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
    public string? AcknowledgedBy { get; set; }
    public string? AcknowledgePayload { get; set; }
}
