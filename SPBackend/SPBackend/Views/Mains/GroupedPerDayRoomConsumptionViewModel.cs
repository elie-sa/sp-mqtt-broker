namespace SPBackend.Views.Mains;

public class GroupedPerDayRoomConsumptionViewModel
{
    public List<GroupedRoomConsumption> GroupedRooms { get; set; }
}

public class GroupedRoomConsumption
{
    public string RoomType { get; set; }
    public long Consumption { get; set; }
}