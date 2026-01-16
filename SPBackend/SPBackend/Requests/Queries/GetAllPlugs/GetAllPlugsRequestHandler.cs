using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetAllPlugs;

public class GetAllPlugsRequestHandler: IRequestHandler<GetAllPlugsRequest, GetAllPlugsResponse>
{
    private readonly PlugsService _plugsService;

    public GetAllPlugsRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetAllPlugsResponse> Handle(GetAllPlugsRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetAllPlugs(request, cancellationToken);
    }
}