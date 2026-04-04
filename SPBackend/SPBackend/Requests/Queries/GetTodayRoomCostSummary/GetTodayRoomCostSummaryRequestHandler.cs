using MediatR;
using SPBackend.Services.Rooms;

namespace SPBackend.Requests.Queries.GetTodayRoomCostSummary;

public class GetTodayRoomCostSummaryRequestHandler : IRequestHandler<GetTodayRoomCostSummaryRequest, GetTodayRoomCostSummaryResponse>
{
    private readonly RoomsService _roomsService;

    public GetTodayRoomCostSummaryRequestHandler(RoomsService roomsService)
    {
        _roomsService = roomsService;
    }

    public async Task<GetTodayRoomCostSummaryResponse> Handle(GetTodayRoomCostSummaryRequest request, CancellationToken cancellationToken)
    {
        return await _roomsService.GetTodayRoomCostSummary(cancellationToken);
    }
}
