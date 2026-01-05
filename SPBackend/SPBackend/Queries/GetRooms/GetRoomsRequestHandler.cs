using MediatR;
using SPBackend.Services.Rooms;

namespace SPBackend.Queries.GetRooms;

public class GetRoomsRequestHandler: IRequestHandler<GetRoomsRequest, GetRoomsResponse>
{
    private readonly RoomsService _roomsService;

    public GetRoomsRequestHandler(RoomsService roomsService)
    {
        _roomsService = roomsService;
    }

    public async Task<GetRoomsResponse> Handle(GetRoomsRequest request, CancellationToken cancellationToken)
    {
        return await _roomsService.GetAllRooms(cancellationToken);
    }
}