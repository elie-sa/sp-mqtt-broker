using MediatR;
using Moq;
using SPBackend.Controllers;
using SPBackend.Requests.Queries.GetPlugsPerRoom;
using SPBackend.Requests.Queries.GetRooms;

namespace SPBackend.Tests.Controllers;

public class RoomControllerTests
{
    [Fact]
    public async Task GetRooms_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetRoomsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRoomsResponse());

        var controller = new RoomController(mediator.Object);
        var result = await controller.GetRooms();

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPlugsPerRoom_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPlugsPerRoomRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPlugsPerRoomResponse());

        var controller = new RoomController(mediator.Object);
        var result = await controller.GetPlugsPerRoom(1);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
