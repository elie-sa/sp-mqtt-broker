namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceSessionHours;

public class GetWeeklyPowerSourceSessionHoursResponse
{
    public string PowerSourceName { get; set; } = string.Empty;
    public List<DailyPowerSourceHours> Days { get; set; } = new();
}

public class DailyPowerSourceHours
{
    public string Day { get; set; }
    public double Hours { get; set; }
}
