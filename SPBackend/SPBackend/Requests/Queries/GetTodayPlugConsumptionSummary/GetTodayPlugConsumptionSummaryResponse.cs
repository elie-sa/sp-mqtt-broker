namespace SPBackend.Requests.Queries.GetTodayPlugConsumptionSummary;

public class GetTodayPlugConsumptionSummaryResponse
{
    public List<PlugConsumptionSummaryItem> Plugs { get; set; } = new();
}

public class PlugConsumptionSummaryItem
{
    public string Name { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Kwh { get; set; }
}
