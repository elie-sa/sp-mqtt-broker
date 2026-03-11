using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Commands.AddNotificationToken;

namespace SPBackend.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationsController: ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("token")]
    // TODO: check FromQuery or FromBody better?
    public async Task<IActionResult> AddNotificationToken([FromQuery] AddNotificationTokenRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}