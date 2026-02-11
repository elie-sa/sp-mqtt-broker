namespace SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;

public class GetGroupedPerDayRoomConsumptionResponse
{
    public List<GroupedRoomConsumption> GroupedRooms { get; set; }
}
public class GroupedRoomConsumption
{
    public string RoomType { get; set; }
    public double Consumption { get; set; }
}