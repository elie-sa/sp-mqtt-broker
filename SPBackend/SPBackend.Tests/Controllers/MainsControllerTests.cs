using MediatR;
using Moq;
using SPBackend.Controllers;
using SPBackend.Requests.Queries.GetAllSources;
using SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPlugsPerRoomOverview;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Services.Mains;

namespace SPBackend.Tests.Controllers;

public class MainsControllerTests
{
    [Fact]
    public async Task GetSource_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPowerSourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPowerSourceResponse());
        var powerSourceService = new Mock<PowerSourceService>(null!, null!);

        var controller = new MainsController(powerSourceService.Object, mediator.Object);
        var result = await controller.GetSource();

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSources_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAllSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAllSourcesResponse());
        var powerSourceService = new Mock<PowerSourceService>(null!, null!);

        var controller = new MainsController(powerSourceService.Object, mediator.Object);
        var result = await controller.GetAllSources();

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPerDayRoomsConsumptions_Grouped_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetGroupedPerDayRoomConsumptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetGroupedPerDayRoomConsumptionResponse());
        var powerSourceService = new Mock<PowerSourceService>(null!, null!);

        var controller = new MainsController(powerSourceService.Object, mediator.Object);
        var result = await controller.GetPerDayRoomsConsumptions(true);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPerDayRoomsConsumptions_Ungrouped_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPerDayRoomConsumptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPerDayRoomConsumptionResponse());
        var powerSourceService = new Mock<PowerSourceService>(null!, null!);

        var controller = new MainsController(powerSourceService.Object, mediator.Object);
        var result = await controller.GetPerDayRoomsConsumptions(false);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPlugsPerRoomOverview_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPlugsPerRoomOverviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPlugsPerRoomOverviewResponse());
        var powerSourceService = new Mock<PowerSourceService>(null!, null!);

        var controller = new MainsController(powerSourceService.Object, mediator.Object);
        var result = await controller.GetPlugsPerRoomOverview();

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
