namespace SPBackend.Models;

public class Mains
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public long Voltage { get; set; }
    public long Amperage { get; set; }
    public long PowerSourceId { get; set; }
    public PowerSource PowerSource { get; set; }
}