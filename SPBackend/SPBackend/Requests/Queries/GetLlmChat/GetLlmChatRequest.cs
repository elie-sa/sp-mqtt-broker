using MediatR;

namespace SPBackend.Requests.Queries.GetLlmChat;

public class GetLlmChatRequest: IRequest<GetLlmChatResponse>
{
    public string Prompt { get; set; } = string.Empty;
}