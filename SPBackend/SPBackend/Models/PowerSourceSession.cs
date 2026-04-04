namespace SPBackend.Models;

public class PowerSourceSession
{
    public long Id { get; set; }
    public long PowerSourceId { get; set; }
    public PowerSource PowerSource { get; set; }
    public long HouseholdId { get; set; }
    public Household Household { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
