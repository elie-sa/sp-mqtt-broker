using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Commands.UpdatePowerSourceCost;

namespace SPBackend.Controllers;

[ApiController]
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPut("powersources/cost")]
    public async Task<IActionResult> UpdatePowerSourceCost([FromBody] UpdatePowerSourceCostRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
