using System.Text.Json.Serialization;
using MediatR;

namespace SPBackend.Requests.Commands.SendNotification;

public class SendNotificationRequest: IRequest<SendNotificationResponse>
    
{
    [JsonPropertyName("to")]
    public string To { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("body")]
    public string Body { get; set; }
    // TODO: could be expanded by adding data, subtitle (iOS) or channel ID (android)...
}