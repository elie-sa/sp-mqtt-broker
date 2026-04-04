namespace SPBackend.Requests.Queries.GetTodayPlugCostSummary;

public class GetTodayPlugCostSummaryResponse
{
    public List<PlugCostSummaryItem> Plugs { get; set; } = new();
}

public class PlugCostSummaryItem
{
    public string Name { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Cost { get; set; }
}
