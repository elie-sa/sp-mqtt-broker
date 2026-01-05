namespace SPBackend.Queries.GetGroupedPerDayRoomConsumption;

public class GetGroupedPerDayRoomConsumptionResponse
{
    public List<GroupedRoomConsumption> GroupedRooms { get; set; }
}
public class GroupedRoomConsumption
{
    public string RoomType { get; set; }
    public long Consumption { get; set; }
}