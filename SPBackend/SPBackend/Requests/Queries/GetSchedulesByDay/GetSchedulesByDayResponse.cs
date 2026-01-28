namespace SPBackend.Requests.Queries.GetSchedulesByDay;

public class GetSchedulesByDayResponse
{
    public List<ScheduleDto> Schedules { get; set; } = new();
}
public sealed class ScheduleDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public bool IsActive { get; set; }
    public int DeviceCount { get; set; }
}