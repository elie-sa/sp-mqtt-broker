using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetPolicy;

public class GetPolicyRequestHandler: IRequestHandler<GetPolicyRequest, GetPolicyResponse>
{
    private readonly PlugsService _plugsService;

    public GetPolicyRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetPolicyResponse> Handle(GetPolicyRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetPolicy(request.PolicyId, cancellationToken);
    }
}