using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;

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
}
