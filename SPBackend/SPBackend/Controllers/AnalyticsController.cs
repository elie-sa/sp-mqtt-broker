using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;
using SPBackend.Requests.Queries.GetTodayPlugConsumptionSummary;
using SPBackend.Requests.Queries.GetTodayRoomConsumptionSummary;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceUsage;

namespace SPBackend.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("mains/monthly")]
    public async Task<IActionResult> GetMonthlyConsumptionSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetMonthlyConsumptionSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("mains/weekly/percentages")]
    public async Task<IActionResult> GetWeeklyPowerSourceUsage(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetWeeklyPowerSourceUsageRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("rooms/daily")]
    public async Task<IActionResult> GetTodayRoomConsumptionSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayRoomConsumptionSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("plugs/daily")]
    public async Task<IActionResult> GetTodayPlugConsumptionSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayPlugConsumptionSummaryRequest(), cancellationToken));
    }

}
