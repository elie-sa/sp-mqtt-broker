using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Commands.AddPolicy;

namespace SPBackend.Controllers;

[ApiController]
[Route("policy")]
public class PolicyController: ControllerBase
{
    private readonly IMediator _mediator;

    public PolicyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> AddPolicy(AddPolicyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}