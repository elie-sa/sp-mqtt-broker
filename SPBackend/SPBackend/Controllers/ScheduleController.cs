using System.Runtime.InteropServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Queries.GetScheduleDetails;
using SPBackend.Queries.GetSchedules;

namespace SPBackend.Controllers;

[ApiController]
[Route("schedules")]
public class ScheduleController: ControllerBase
{
    private readonly IMediator _mediator;

    public ScheduleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetSchedules(CancellationToken cancellationToken, [FromQuery] int pageSize, [FromQuery] int page = 1)
    {
        return Ok(await _mediator.Send(new GetSchedulesRequest(){Page = page, PageSize = pageSize}, cancellationToken));
    }

    [HttpGet("{scheduleId}")]
    [Authorize]
    public async Task<IActionResult> GetSchedule(int scheduleId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetScheduleDetailsRequest(){ ScheduleId = scheduleId}, cancellationToken));
    }
}