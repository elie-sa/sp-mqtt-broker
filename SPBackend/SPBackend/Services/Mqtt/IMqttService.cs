namespace SPBackend.Services.Mqtt;

public interface IMqttService
{
    Task ConnectAsync();
    Task PublishAsync(string topic, string message);
}
