namespace SPBackend.Models;

public class MainsLog
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public long Voltage { get; set; }
    public long Amperage { get; set; }
    public long PowerSourceId { get; set; }
    public PowerSource PowerSource { get; set; }
    public long HouseholdId { get; set; }
    public Household Household { get; set; }
}