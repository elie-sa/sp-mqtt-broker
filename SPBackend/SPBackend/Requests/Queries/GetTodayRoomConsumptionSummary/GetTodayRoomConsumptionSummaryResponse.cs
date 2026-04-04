namespace SPBackend.Requests.Queries.GetTodayRoomConsumptionSummary;

public class GetTodayRoomConsumptionSummaryResponse
{
    public List<RoomConsumptionSummaryItem> Rooms { get; set; } = new();
}

public class RoomConsumptionSummaryItem
{
    public string Name { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Kwh { get; set; }
}
