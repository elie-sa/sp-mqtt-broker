using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Commands.RemovePlugFromSchedule;
using SPBackend.Commands.SetPlug;
using SPBackend.Commands.SetPlugName;
using SPBackend.Queries.GetPlugDetails;
using SPBackend.Services.MQTTService;

namespace SPBackend.Controllers;

[ApiController]
[Route("plugs")]
public class PlugController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly MqttService _mqttService;

    public PlugController(MqttService mqttService, IMediator mediator)
    {
        _mqttService = mqttService;
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("{plugId}")]
    public async Task<IActionResult> GetPlug(long plugId)
    {
        return Ok(await _mediator.Send(new GetPlugDetailsRequest(){ PlugId = plugId }));
    }
    
    //Test endpoint
    [Authorize]
    [HttpPost("testPublish")]
    public async Task<IActionResult> Publish([FromQuery] string topic, [FromQuery] string message)
    {
        await _mqttService.ConnectAsync();
        await _mqttService.PublishAsync(topic, message);
        return Ok($"Published {message}");
    }

    [Authorize]
    [HttpPut("status/set")]
    public async Task<IActionResult> SetPlug(SetPlugRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [Authorize]
    [HttpDelete("{plugId}/schedules/{scheduleId}")]
    public async Task<IActionResult> RemovePlugFromSchedule(long plugId, long scheduleId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new RemovePlugFromScheduleRequest(){PlugId = plugId, ScheduleId = scheduleId}, cancellationToken));
    }

    [Authorize]
    [HttpPut("name/set")]
    public async Task<IActionResult> SetPlugName(SetPlugNameRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
    
}