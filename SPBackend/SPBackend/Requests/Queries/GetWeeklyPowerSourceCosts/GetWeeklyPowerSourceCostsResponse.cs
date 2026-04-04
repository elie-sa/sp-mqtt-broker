namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceCosts;

public class GetWeeklyPowerSourceCostsResponse
{
    public List<DailyPowerSourceCosts> Days { get; set; } = new();
}

public class DailyPowerSourceCosts
{
    public string Day { get; set; }
    public List<PowerSourceDailyCost> PowerSources { get; set; } = new();
}

public class PowerSourceDailyCost
{
    public string Name { get; set; } = string.Empty;
    public double Cost { get; set; }
}
