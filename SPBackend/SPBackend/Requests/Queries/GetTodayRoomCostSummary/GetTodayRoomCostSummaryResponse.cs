namespace SPBackend.Requests.Queries.GetTodayRoomCostSummary;

public class GetTodayRoomCostSummaryResponse
{
    public List<RoomCostSummaryItem> Rooms { get; set; } = new();
}

public class RoomCostSummaryItem
{
    public string Name { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Cost { get; set; }
}
