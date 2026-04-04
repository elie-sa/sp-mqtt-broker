namespace SPBackend.Models;

public class MainsConsumptions
{
    public long Id { get; set; }
    public DateOnly Time { get; set; }
    public double Consumption { get; set; }
    public PowerSource PowerSource { get; set; }
    public long PowerSourceId { get; set; }
}