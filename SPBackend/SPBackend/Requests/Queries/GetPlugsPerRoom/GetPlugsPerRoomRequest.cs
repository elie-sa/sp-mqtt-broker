using MediatR;

namespace SPBackend.Requests.Queries.GetPlugsPerRoom;

public class GetPlugsPerRoomRequest: IRequest<GetPlugsPerRoomResponse>
{
    public long RoomId { get; set; }
}