using MediatR;
using SPBackend.Services.Rooms;

namespace SPBackend.Requests.Queries.GetTodayRoomConsumptionSummary;

public class GetTodayRoomConsumptionSummaryRequestHandler : IRequestHandler<GetTodayRoomConsumptionSummaryRequest, GetTodayRoomConsumptionSummaryResponse>
{
    private readonly RoomsService _roomsService;

    public GetTodayRoomConsumptionSummaryRequestHandler(RoomsService roomsService)
    {
        _roomsService = roomsService;
    }

    public async Task<GetTodayRoomConsumptionSummaryResponse> Handle(GetTodayRoomConsumptionSummaryRequest request, CancellationToken cancellationToken)
    {
        return await _roomsService.GetTodayRoomConsumptionSummary(cancellationToken);
    }
}
