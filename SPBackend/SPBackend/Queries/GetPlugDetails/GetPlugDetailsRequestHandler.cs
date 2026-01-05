using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Queries.GetPlugDetails;

public class GetPlugDetailsRequestHandler: IRequestHandler<GetPlugDetailsRequest, GetPlugDetailsResponse>
{
    private readonly PlugsService _plugsService;

    public GetPlugDetailsRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetPlugDetailsResponse> Handle(GetPlugDetailsRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetPlugDetails(request.PlugId, cancellationToken);
    }
}