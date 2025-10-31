using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SPBackend.Models;

public class Schedule
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public ICollection<PlugControl> PlugControls { get; set; }
}