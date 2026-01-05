using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Queries.GetPerDayRoomConsumption;

public class GetPerDayRoomConsumptionRequestHandler: IRequestHandler<GetPerDayRoomConsumptionRequest, GetPerDayRoomConsumptionResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetPerDayRoomConsumptionRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetPerDayRoomConsumptionResponse> Handle(GetPerDayRoomConsumptionRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetPerDayRoomConsumption();
    }
}