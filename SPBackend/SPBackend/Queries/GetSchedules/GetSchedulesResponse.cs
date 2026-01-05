namespace SPBackend.Queries.GetSchedules;

public class GetSchedulesResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<ScheduleDto> Schedules { get; set; } = new();      
}

public sealed class ScheduleDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public int DeviceCount { get; set; }
}