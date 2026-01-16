using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetPerDayRoomConsumption;

public class GetPerDayRoomConsumptionResponse
{
    public List<RoomConsumption> Rooms { get; set; }
}