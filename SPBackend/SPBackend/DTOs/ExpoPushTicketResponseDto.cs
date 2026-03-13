using System.Text.Json;

namespace SPBackend.DTOs;

public class ExpoPushTicketResponseDto
{
    public ExpoPushTicketDto? Data { get; set; }
    public List<ExpoPushErrorDto>? Errors { get; set; }
}

public class ExpoPushTicketDto
{
    public string? Status { get; set; }
    public string? Id { get; set; }
    public string? Message { get; set; }
    public JsonElement? Details { get; set; }
}

public class ExpoPushErrorDto
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}