using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPlugsPerRoomOverview;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Services.Mains;

namespace SPBackend.Controllers;

[ApiController]
[Route("mains")]
public class MainsController: ControllerBase
{
    private readonly IMediator _mediator;
    private readonly PowerSourceService _powerSourceService;

    public MainsController(PowerSourceService powerSourceService, IMediator mediator)
    {
        _powerSourceService = powerSourceService;
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("source")]
    public async Task<IActionResult> GetSource()
    {
        return Ok(await _mediator.Send(new GetPowerSourceRequest()));
    }
    
    [Authorize]
    [HttpGet("rooms/consumption/daily")]
    public async Task<IActionResult> GetPerDayRoomsConsumptions([FromQuery] bool groupByRoomType = false)
    {
        if (groupByRoomType)
        {
            return Ok(await _mediator.Send(new GetGroupedPerDayRoomConsumptionRequest()));
        }

        return Ok(await _mediator.Send(new GetPerDayRoomConsumptionRequest()));
    }
    
    [Authorize]
    [HttpGet("rooms/plugs/details")]
    public async Task<IActionResult> GetPlugsPerRoomOverview()
    {
        return Ok(await _mediator.Send(new GetPlugsPerRoomOverviewRequest()));
    }
}