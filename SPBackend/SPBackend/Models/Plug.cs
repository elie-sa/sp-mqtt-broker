namespace SPBackend.Models;

public class Plug
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }
    public bool IsConstant { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Room Room { get; set; }
    public long RoomId { get; set; }
    public DeviceType DeviceType { get; set; }
    public long DeviceTypeId { get; set; }
    public ICollection<PlugControl> PlugControls { get; set; }
    public ICollection<Consumption> Consumptions { get; set; }
}