using MediatR;
using SPBackend.Services.LLM;

namespace SPBackend.Requests.Queries.GetLlmChat;

public class GetLlmChatRequestHandler : IRequestHandler<GetLlmChatRequest, GetLlmChatResponse>
{
    private readonly GeminiService _geminiService;

    public GetLlmChatRequestHandler(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<GetLlmChatResponse> Handle(
        GetLlmChatRequest request,
        CancellationToken cancellationToken)
    {
        return await _geminiService.GetResponse(request.Prompt);
    }
}