using MediatR;
using SPBackend.Services.LLM;

namespace SPBackend.Requests.Commands.SendLlmChat;

public class SendLlmChatRequestHandler : IRequestHandler<SendLlmChatRequest, SendLlmChatResponse>
{
    private readonly GeminiService _geminiService;

    public SendLlmChatRequestHandler(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<SendLlmChatResponse> Handle(
        SendLlmChatRequest request,
        CancellationToken cancellationToken)
    {
        return await _geminiService.GetResponse(request);
    }
}