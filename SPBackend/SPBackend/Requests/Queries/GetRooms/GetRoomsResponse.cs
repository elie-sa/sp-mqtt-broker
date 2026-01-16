using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetRooms;

public class GetRoomsResponse
{
    public List<RoomDetails> Rooms { get; set; }
}