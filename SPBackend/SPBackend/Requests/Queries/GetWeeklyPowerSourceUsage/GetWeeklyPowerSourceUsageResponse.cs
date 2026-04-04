namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceUsage;

public class GetWeeklyPowerSourceUsageResponse
{
    public List<PowerSourceUsagePercentage> PowerSources { get; set; } = new();
}

public class PowerSourceUsagePercentage
{
    public string Name { get; set; } = string.Empty;
    public double Percentage { get; set; }
}
