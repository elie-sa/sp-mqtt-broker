using Microsoft.Extensions.Hosting;

namespace SPBackend.Services.Mqtt;

public class MqttHostedService : IHostedService
{
    private readonly IMqttService _mqttService;

    public MqttHostedService(IMqttService mqttService)
    {
        _mqttService = mqttService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttService.ConnectAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
