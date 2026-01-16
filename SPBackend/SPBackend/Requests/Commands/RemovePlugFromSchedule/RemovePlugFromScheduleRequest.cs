using MediatR;

namespace SPBackend.Requests.Commands.RemovePlugFromSchedule;

public class RemovePlugFromScheduleRequest: IRequest<RemovePlugFromScheduleResponse>
{
    public long PlugId { get; set; }
    public long ScheduleId { get; set; }
}