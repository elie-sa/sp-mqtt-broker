using MediatR;
using Moq;
using SPBackend.Controllers;
using SPBackend.Requests.Commands.AddPolicy;
using SPBackend.Requests.Commands.DeletePolicy;
using SPBackend.Requests.Commands.EditPolicy;
using SPBackend.Requests.Commands.TogglePolicy;
using SPBackend.Requests.Queries.GetAllPolicies;
using SPBackend.Requests.Queries.GetPolicy;

namespace SPBackend.Tests.Controllers;

public class PolicyControllerTests
{
    [Fact]
    public async Task AddPolicy_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<AddPolicyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddPolicyResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.AddPolicy(new AddPolicyRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPolicies_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAllPoliciesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAllPoliciesResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.GetPolicies(CancellationToken.None, false, false);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPolicies_FilterConflict_ReturnsBadRequest()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PolicyController(mediator.Object);

        var result = await controller.GetPolicies(CancellationToken.None, true, true);

        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPolicy_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPolicyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPolicyResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.GetPolicy(1, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task DeletePolicy_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<DeletePolicyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeletePolicyResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.DeletePolicy(1, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task EditPolicy_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<EditPolicyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EditPolicyResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.EditPolicy(new EditPolicyRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }

    [Fact]
    public async Task TogglePolicy_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<TogglePolicyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TogglePolicyResponse());

        var controller = new PolicyController(mediator.Object);
        var result = await controller.TogglePolicy(new TogglePolicyRequest(), CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
