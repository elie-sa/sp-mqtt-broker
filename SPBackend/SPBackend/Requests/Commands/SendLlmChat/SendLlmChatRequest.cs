using MediatR;

namespace SPBackend.Requests.Commands.SendLlmChat;

public class SendLlmChatRequest: IRequest<SendLlmChatResponse>
{
    public string Prompt { get; set; } = string.Empty;
    public string SessionId { get; set; }
}