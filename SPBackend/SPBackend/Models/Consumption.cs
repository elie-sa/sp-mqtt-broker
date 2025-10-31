namespace SPBackend.Models;

public class Consumption
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public long TotalEnergy { get; set; }
    public long PlugId { get; set; }
    public Plug Plug { get; set; }
}