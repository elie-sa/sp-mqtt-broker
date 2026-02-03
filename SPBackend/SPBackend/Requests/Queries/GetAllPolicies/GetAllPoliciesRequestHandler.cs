using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetAllPolicies;

public class GetAllPoliciesRequestHandler: IRequestHandler<GetAllPoliciesRequest, GetAllPoliciesResponse>
{
    private readonly PlugsService _plugsService;

    public GetAllPoliciesRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetAllPoliciesResponse> Handle(GetAllPoliciesRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetAllPolicies(request, cancellationToken);
    }
}