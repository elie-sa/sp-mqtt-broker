using MediatR;

namespace SPBackend.Requests.Commands.ToggleSchedule;

public class ToggleScheduleRequest: IRequest<ToggleScheduleResponse>
{
    public long ScheduleId { get; set; }
    public bool Enable { get; set; }
}