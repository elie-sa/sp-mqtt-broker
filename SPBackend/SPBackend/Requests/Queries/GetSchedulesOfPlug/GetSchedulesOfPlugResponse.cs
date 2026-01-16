using SPBackend.Requests.Queries.GetSchedules;

namespace SPBackend.Requests.Queries.GetSchedulesOfPlug;

public class GetSchedulesOfPlugResponse
{
    public int Page { get; set; } 
    public int PageSize { get; set; }
    public int TotalDays { get; set; }
    public int TotalPages { get; set; }

    public List<PlugScheduleDayDto> Days { get; set; } = new();
}

public class PlugScheduleDayDto
{
    public DateOnly Date { get; set; }
    public List<ScheduleOfPlugDto> Schedules { get; set; } = new();
}

public class ScheduleOfPlugDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public bool IsOn { get; set; }
    public bool IsActive { get; set; }
}   