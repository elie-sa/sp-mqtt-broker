using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Queries.GetLlmChat;

namespace SPBackend.Controllers;

[ApiController]
[Route("llm")]
public class LlmController : ControllerBase
{
    private readonly IMediator _mediator;

    public LlmController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("chat")]
    public async Task<ActionResult<GetLlmChatResponse>> Chat([FromBody] GetLlmChatRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}