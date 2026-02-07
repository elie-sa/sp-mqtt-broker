using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetAllSources;

public class GetAllSourcesRequestHandler: IRequestHandler<GetAllSourcesRequest, GetAllSourcesResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetAllSourcesRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetAllSourcesResponse> Handle(GetAllSourcesRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetAllSources(request, cancellationToken);
    }
}