using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace SPBackend.Tests.Controllers;

public abstract class ControllerTestBase
{
    protected static OkObjectResult OkResult(IActionResult result)
    {
        return Assert.IsType<OkObjectResult>(result);
    }

    protected static Mock<IMediator> CreateMediator(object? response)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<IRequest<object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        return mediator;
    }
}
