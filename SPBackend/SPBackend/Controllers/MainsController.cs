using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Services.Mains;

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

    [HttpGet("source/{householdId}")]
    public async Task<IActionResult> GetSource(int householdId)
    {
        var powerSourceDetails = await _powerSourceService.GetPowerSource(householdId);   
        return Ok(powerSourceDetails);
    }
}