namespace SPBackend.Models;

public class PlugControl
{
    public long Id { get; set; }
    public bool SetStatus { get; set; }
    public long PlugId { get; set; }
    public Plug Plug { get; set; }
    public long ScheduleId { get; set; }
    public Schedule Schedule { get; set; }
}