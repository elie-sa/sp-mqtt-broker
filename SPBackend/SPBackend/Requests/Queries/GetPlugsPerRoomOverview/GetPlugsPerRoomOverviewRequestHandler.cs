using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetPlugsPerRoomOverview;

public class GetPlugsPerRoomOverviewRequestHandler: IRequestHandler<GetPlugsPerRoomOverviewRequest, GetPlugsPerRoomOverviewResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetPlugsPerRoomOverviewRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetPlugsPerRoomOverviewResponse> Handle(GetPlugsPerRoomOverviewRequest overviewRequest, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetTotalRoomDetails();
    }
}