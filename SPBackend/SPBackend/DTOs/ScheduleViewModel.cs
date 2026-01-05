namespace SPBackend.DTOs;

public class ScheduleViewModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public bool SetStatus { get; set; }
}