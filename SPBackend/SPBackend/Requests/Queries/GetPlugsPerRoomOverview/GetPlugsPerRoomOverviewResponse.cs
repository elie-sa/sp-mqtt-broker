using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetPlugsPerRoomOverview;

public class GetPlugsPerRoomOverviewResponse
{
    public List<RoomDetails> Rooms { get; set; }
}

