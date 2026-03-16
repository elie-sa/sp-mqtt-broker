namespace SPBackend.Services.Commands;

public sealed class CommandEnvelope
{
    public Guid Id { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string HttpMethod { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string? UserId { get; init; }
}
