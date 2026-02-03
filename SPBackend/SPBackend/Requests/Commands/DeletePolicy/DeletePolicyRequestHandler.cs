using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.DeletePolicy;

public class DeletePolicyRequestHandler 
    : IRequestHandler<DeletePolicyRequest, DeletePolicyResponse>
{
    private readonly PlugsService _plugsService;

    public DeletePolicyRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<DeletePolicyResponse> Handle(
        DeletePolicyRequest request,
        CancellationToken cancellationToken)
    {
        return await _plugsService.DeletePolicy(request.PolicyId, cancellationToken);
    }
}