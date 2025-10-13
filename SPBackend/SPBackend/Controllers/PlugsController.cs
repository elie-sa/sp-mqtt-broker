using System.Text;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using SPBackend.Services.MQTTService;

namespace SPBackend.Controllers;

[ApiController]
[Route("plugs")]
public class PlugsController : ControllerBase
{
    private readonly MqttService _mqttService;

    public PlugsController(MqttService mqttService)
    {
        _mqttService = mqttService;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromQuery] string topic, [FromQuery] string message)
    {
        await _mqttService.ConnectAsync();
        await _mqttService.PublishAsync(topic, message);
        return Ok($"Published {message}");
    }
    
}