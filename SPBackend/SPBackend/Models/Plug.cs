namespace SPBackend.Models;

public class Plug
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }
    public bool IsConstant { get; set; }
    public TimeSpan? Timeout { get; set; }
    public ICollection<PlugControl> PlugControls { get; set; }
    public ICollection<Consumption> Consumptions { get; set; }
}