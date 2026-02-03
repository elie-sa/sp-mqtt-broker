using SPBackend.Requests.Queries.GetSchedulesByDay;

namespace SPBackend.Requests.Queries.GetSchedulesNextDays;

public class GetSchedulesNextDaysResponse
{
    public List<DaySchedulesDto> Days { get; set; } = new();
}

public class DaySchedulesDto
{
    public DateOnly Date { get; set; }
    public List<ScheduleDto> Schedules { get; set; } = new();
}