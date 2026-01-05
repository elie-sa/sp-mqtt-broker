using SPBackend.DTOs;

namespace SPBackend.Queries.GetRooms;

public class GetRoomsResponse
{
    public List<RoomDetails> Rooms { get; set; }
}