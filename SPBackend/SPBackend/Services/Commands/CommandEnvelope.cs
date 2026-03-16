using System.Text.Json;

namespace SPBackend.Services.Commands;

public sealed class CommandEnvelope
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public JsonElement? Payload { get; set; }
    public string? UserId { get; set; }
}
