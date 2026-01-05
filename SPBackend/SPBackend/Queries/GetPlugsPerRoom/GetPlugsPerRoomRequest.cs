using MediatR;

namespace SPBackend.Queries.GetPlugsPerRoom;

public class GetPlugsPerRoomRequest: IRequest<GetPlugsPerRoomResponse>
{
    public long RoomId { get; set; }
}