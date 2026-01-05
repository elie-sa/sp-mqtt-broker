using SPBackend.DTOs;

namespace SPBackend.Queries.GetPerDayRoomConsumption;

public class GetPerDayRoomConsumptionResponse
{
    public List<RoomConsumption> Rooms { get; set; }
}