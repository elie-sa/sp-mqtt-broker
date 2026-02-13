using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SPBackend.Controllers;
using SPBackend.Requests.Commands.AddTimeout;
using SPBackend.Requests.Commands.DeleteTimeout;
using SPBackend.Requests.Commands.RemovePlugFromSchedule;
using SPBackend.Requests.Commands.SetPlug;
using SPBackend.Requests.Commands.SetPlugName;
using SPBackend.Requests.Queries.GetAllPlugs;
using SPBackend.Requests.Queries.GetPlugDetails;
using SPBackend.Services.Mqtt;

namespace SPBackend.Tests.Controllers;

public class PlugControllerTests
{
    [Fact]
    public async Task GetAllPlugs_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAllPlugsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAllPlugsResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.GetAllPlugs(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPlug_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPlugDetailsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPlugDetailsResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.GetPlug(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Publish_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.Publish("topic", "message");

        Assert.IsType<OkObjectResult>(result);
        mqtt.Verify(m => m.ConnectAsync(), Times.Once);
        mqtt.Verify(m => m.PublishAsync("topic", "message"), Times.Once);
    }

    [Fact]
    public async Task SetPlug_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<SetPlugRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SetPlugResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.SetPlug(new SetPlugRequest(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RemovePlugFromSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<RemovePlugFromScheduleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RemovePlugFromScheduleResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.RemovePlugFromSchedule(1, 2, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SetPlugName_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<SetPlugNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SetPlugNameResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.SetPlugName(new SetPlugNameRequest(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SetPlugTimeout_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<AddTimeoutRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddTimeoutResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.SetPlugTimeout(new AddTimeoutRequest(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeletePlugTimeout_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<DeleteTimeoutRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteTimeoutResponse());
        var mqtt = new Mock<IMqttService>();

        var controller = new PlugController(mqtt.Object, mediator.Object);
        var result = await controller.DeletePlugTimeout(1, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }
}
