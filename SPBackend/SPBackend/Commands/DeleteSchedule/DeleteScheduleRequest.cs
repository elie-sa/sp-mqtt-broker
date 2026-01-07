using MediatR;

namespace SPBackend.Commands.DeleteSchedule;

public class DeleteScheduleRequest: IRequest<DeleteScheduleResponse>
{
    public long ScheduleId { get; set; }
}