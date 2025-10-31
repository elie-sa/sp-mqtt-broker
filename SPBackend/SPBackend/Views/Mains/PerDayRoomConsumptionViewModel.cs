namespace SPBackend.Views.Mains;

public class PerDayRoomConsumptionViewModel
{
    public List<RoomConsumption> Rooms { get; set; }
}

public class RoomConsumption
{
    public long RoomId { get; set; }
    public string Name { get; set; }
    public string RoomType { get; set; }
    public long Consumption { get; set; }
}