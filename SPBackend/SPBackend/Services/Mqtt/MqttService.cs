using System.Text;
using MQTTnet;
using MQTTnet.Client;

namespace SPBackend.Services.MQTTService;

public class MqttService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public MqttService()
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptions
        {
            ClientId = "backend-server",
            CleanSession = true,
            ChannelOptions = new MqttClientTcpOptions
            {
                Server = "localhost",
                Port = 1883
            }
        };

        _client.DisconnectedAsync += async e =>
        {
            await Task.Delay(5000);
            try { await _client.ConnectAsync(_options); } catch { }
        };
    }

    public async Task ConnectAsync()
    {
        if (!_client.IsConnected)
        {
            await _client.ConnectAsync(_options);
            await _client.SubscribeAsync("sample_topic");

            _client.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    
                Console.WriteLine($"Received {payload} on {topic}");
                return Task.CompletedTask;
            };
        }
    }

    public Task PublishAsync(string topic, string message)
    {
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        return _client.PublishAsync(msg);
    }
}