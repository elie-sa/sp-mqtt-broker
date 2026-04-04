using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;
using SPBackend.Requests.Queries.GetMonthlyPowerSourceBill;
using SPBackend.Requests.Queries.GetTodayPlugConsumptionSummary;
using SPBackend.Requests.Queries.GetTodayPlugCostSummary;
using SPBackend.Requests.Queries.GetTodayRoomConsumptionSummary;
using SPBackend.Requests.Queries.GetTodayRoomCostSummary;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceSessionHours;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceCosts;
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
    [HttpGet("rooms/daily/consumptions")]
    public async Task<IActionResult> GetTodayRoomConsumptionSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayRoomConsumptionSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("plugs/daily/consumptions")]
    public async Task<IActionResult> GetTodayPlugConsumptionSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayPlugConsumptionSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("rooms/daily/costs")]
    public async Task<IActionResult> GetTodayRoomCostSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayRoomCostSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("plugs/daily/costs")]
    public async Task<IActionResult> GetTodayPlugCostSummary(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTodayPlugCostSummaryRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("mains/monthly/bill")]
    public async Task<IActionResult> GetMonthlyPowerSourceBill(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetMonthlyPowerSourceBillRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("mains/weekly/costs")]
    public async Task<IActionResult> GetWeeklyPowerSourceCosts(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetWeeklyPowerSourceCostsRequest(), cancellationToken));
    }

    [Authorize]
    [HttpGet("mains/weekly/powersources/{powerSourceId}/hours")]
    public async Task<IActionResult> GetWeeklyPowerSourceSessionHours(long powerSourceId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetWeeklyPowerSourceSessionHoursRequest { PowerSourceId = powerSourceId }, cancellationToken));
    }

}
