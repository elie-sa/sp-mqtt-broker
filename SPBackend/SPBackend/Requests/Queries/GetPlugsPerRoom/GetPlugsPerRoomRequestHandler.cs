using MediatR;
using SPBackend.Services.Rooms;

namespace SPBackend.Requests.Queries.GetPlugsPerRoom;

public class GetPlugsPerRoomRequestHandler: IRequestHandler<GetPlugsPerRoomRequest, GetPlugsPerRoomResponse>
{
    private readonly RoomsService _roomsService;

    public GetPlugsPerRoomRequestHandler(RoomsService roomsService)
    {
        _roomsService = roomsService;
    }

    public async Task<GetPlugsPerRoomResponse> Handle(GetPlugsPerRoomRequest request, CancellationToken cancellationToken)
    {
        return await _roomsService.GetPlugsPerRoom(request.RoomId, cancellationToken);
    }
}