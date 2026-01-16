using MediatR;

namespace SPBackend.Requests.Commands.DeleteSchedule;

public class DeleteScheduleRequest: IRequest<DeleteScheduleResponse>
{
    public long ScheduleId { get; set; }
}