using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.AddPolicy;

public class AddPolicyRequestHandler: IRequestHandler<AddPolicyRequest, AddPolicyResponse>
{
    private readonly PlugsService _plugsService;

    public AddPolicyRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<AddPolicyResponse> Handle(AddPolicyRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.AddPolicy(request, cancellationToken);
    }
}