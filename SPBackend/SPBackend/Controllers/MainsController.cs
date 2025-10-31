using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Services.Mains;
using SPBackend.Views.Mains;

namespace SPBackend.Controllers;

[ApiController]
[Route("mains")]
public class MainsController: ControllerBase
{
    private readonly PowerSourceService _powerSourceService;

    public MainsController(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    [HttpGet("{householdId}/source")]
    public async Task<IActionResult> GetSource(int householdId)
    {
        var powerSourceDetails = await _powerSourceService.GetPowerSource(householdId);   
        return Ok(powerSourceDetails);
    }
    
    [HttpGet("households/{householdId}/rooms/consumption/daily")]
    public async Task<IActionResult> GetPerDayRoomsConsumptions(int householdId, [FromQuery] bool groupByRoomType = false)
    {
        if (groupByRoomType)
        {
            var groupedPerDayRoomConsumption = await _powerSourceService.GetGroupedPerDayRoomConsumption(householdId);
            return Ok(groupedPerDayRoomConsumption);
        }
        else
        {
            var perDayRoomsConsumption = await _powerSourceService.GetPerDayRoomConsumption(householdId);
            return Ok(perDayRoomsConsumption);   
        }
    }
    
    [HttpGet("households/{householdId}/rooms/plugs/details")]
    public async Task<IActionResult> GetPlugsPerRoom(int householdId)
    {
        var perRoomPlugs = await _powerSourceService.GetTotalRoomDetails(householdId);   
        return Ok(perRoomPlugs);
    }
    
    
}