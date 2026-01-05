using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Queries.GetSchedules;

public class GetSchedulesRequestHandler: IRequestHandler<GetSchedulesRequest, GetSchedulesResponse>
{
    private readonly PlugsService _plugsService;

    public GetSchedulesRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetSchedulesResponse> Handle(GetSchedulesRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetSchedules(cancellationToken, request.Page, request.PageSize);
    }
}