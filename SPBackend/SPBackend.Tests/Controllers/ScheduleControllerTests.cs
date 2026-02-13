using MediatR;
using Moq;
using SPBackend.Controllers;
using SPBackend.Requests.Commands.AddSchedule;
using SPBackend.Requests.Commands.DeleteSchedule;
using SPBackend.Requests.Commands.EditSchedule;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Requests.Queries.GetScheduleDetails;
using SPBackend.Requests.Queries.GetSchedules;
using SPBackend.Requests.Queries.GetSchedulesByDay;
using SPBackend.Requests.Queries.GetSchedulesNextDays;

namespace SPBackend.Tests.Controllers;

public class ScheduleControllerTests : ControllerTestBase
{
    [Fact]
    public async Task GetSchedules_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetSchedulesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSchedulesResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.GetSchedules(new GetSchedulesRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetSchedulesByDay_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetSchedulesByDayRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSchedulesByDayResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.GetSchedulesByDay(new GetSchedulesByDayRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetSchedulesNextDays_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetSchedulesNextDaysRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSchedulesNextDaysResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.GetSchedulesNextDays(null, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetScheduleDetailsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetScheduleDetailsResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.GetSchedule(1, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task AddSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<AddScheduleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddScheduleResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.AddSchedule(new AddScheduleRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<DeleteScheduleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteScheduleResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.DeleteSchedule(1, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task EditSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<EditScheduleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EditScheduleResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.EditSchedule(new EditScheduleRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task ToggleSchedule_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ToggleScheduleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToggleScheduleResponse());

        var controller = new ScheduleController(mediator.Object);
        var result = await controller.ToggleSchedule(new ToggleScheduleRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
