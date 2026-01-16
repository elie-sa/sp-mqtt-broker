using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetSchedulesOfPlug;

public class GetSchedulesOfPlugRequestHandler: IRequestHandler<GetSchedulesOfPlugRequest, GetSchedulesOfPlugResponse>
{
    private readonly PlugsService _plugsService;

    public GetSchedulesOfPlugRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetSchedulesOfPlugResponse> Handle(GetSchedulesOfPlugRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetSchedulesOfPlug(request.PlugId, cancellationToken, request.Page, request.PageSize);
    }
}