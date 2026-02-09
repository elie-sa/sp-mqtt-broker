using System.Runtime.InteropServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Commands.AddSchedule;
using SPBackend.Requests.Commands.DeleteSchedule;
using SPBackend.Requests.Commands.EditSchedule;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Requests.Queries.GetScheduleDetails;
using SPBackend.Requests.Queries.GetSchedules;
using SPBackend.Requests.Queries.GetSchedulesByDay;
using SPBackend.Requests.Queries.GetSchedulesNextDays;

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
    public async Task<IActionResult> GetSchedules([FromQuery] GetSchedulesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
    
    [HttpGet("day")]
    [Authorize]
    public async Task<IActionResult> GetSchedulesByDay([FromQuery] GetSchedulesByDayRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
    
    [HttpGet("days/upcoming")]
    [Authorize]
    public async Task<IActionResult> GetSchedulesNextDays([FromQuery] long? plugId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSchedulesNextDaysRequest(){ PlugId = plugId}, cancellationToken));
    }

    [HttpGet("{scheduleId}")]
    [Authorize]
    public async Task<IActionResult> GetSchedule(int scheduleId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetScheduleDetailsRequest(){ ScheduleId = scheduleId}, cancellationToken));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> AddSchedule(AddScheduleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{scheduleId}")]
    [Authorize]
    public async Task<IActionResult> DeleteSchedule(int scheduleId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new DeleteScheduleRequest(){ ScheduleId = scheduleId}, cancellationToken));
    }

    [HttpPut("")]
    [Authorize]
    public async Task<IActionResult> EditSchedule(EditScheduleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPut("toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleSchedule(ToggleScheduleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
