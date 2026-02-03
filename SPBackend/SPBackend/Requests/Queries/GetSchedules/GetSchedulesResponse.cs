using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetSchedules;

public class GetSchedulesResponse
{
    public List<DateOnly> ScheduledDates { get; set; } = new();
}