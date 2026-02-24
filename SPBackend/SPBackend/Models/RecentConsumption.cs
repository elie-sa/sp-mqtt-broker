namespace SPBackend.Models;

public class RecentConsumption
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public double TotalEnergy { get; set; }
    public double? Temperature { get; set; }
    public long PlugId { get; set; }
    public Plug Plug { get; set; }
}
