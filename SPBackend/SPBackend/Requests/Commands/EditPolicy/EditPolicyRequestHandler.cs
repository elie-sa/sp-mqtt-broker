using MediatR;
using SPBackend.Requests.Commands.DeletePolicy;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.EditPolicy;

public class EditPolicyRequestHandler :IRequestHandler<EditPolicyRequest, EditPolicyResponse>
{
    private readonly PlugsService _plugsService;

    public EditPolicyRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<EditPolicyResponse> Handle(EditPolicyRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.EditPolicy(request, cancellationToken);
    }
}