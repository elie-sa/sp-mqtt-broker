using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;

public class GetGroupedPerDayRoomConsumptionRequestHandler: IRequestHandler<GetGroupedPerDayRoomConsumptionRequest, GetGroupedPerDayRoomConsumptionResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetGroupedPerDayRoomConsumptionRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetGroupedPerDayRoomConsumptionResponse> Handle(GetGroupedPerDayRoomConsumptionRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetGroupedPerDayRoomConsumption();
    }
}