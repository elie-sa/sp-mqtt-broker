using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Requests.Commands.AddPolicy;
using SPBackend.Requests.Commands.DeletePolicy;
using SPBackend.Requests.Commands.EditPolicy;
using SPBackend.Requests.Commands.TogglePolicy;
using SPBackend.Requests.Queries.GetAllPolicies;
using SPBackend.Requests.Queries.GetPolicy;

namespace SPBackend.Controllers;

[ApiController]
[Route("policy")]
public class PolicyController: ControllerBase
{
    private readonly IMediator _mediator;

    public PolicyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> AddPolicy(AddPolicyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetPolicies(CancellationToken cancellationToken, [FromQuery] bool filterByTemp = false, [FromQuery] bool filterByPowerSource = false)
    {
        if (filterByPowerSource && filterByTemp) return BadRequest("Can't filter by temperature and powersource simultaneously");
        return Ok(await _mediator.Send(new GetAllPoliciesRequest(){ TempOnly = filterByTemp, PowerSourceOnly = filterByPowerSource }, cancellationToken));
    }

    [HttpGet("{policyId}")]
    [Authorize]
    public async Task<IActionResult> GetPolicy(long policyId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetPolicyRequest(){PolicyId = policyId}, cancellationToken));
    }
    
    [HttpDelete("{policyId}")]
    [Authorize]
    public async Task<IActionResult> DeletePolicy(int policyId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new DeletePolicyRequest(){ PolicyId = policyId}, cancellationToken));
    }

    [HttpPut("")]
    [Authorize]
    public async Task<IActionResult> EditPolicy(EditPolicyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPut("toggle")]
    [Authorize]
    public async Task<IActionResult> TogglePolicy(TogglePolicyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}