using MediatR;
using SPBackend.Queries.GetSchedules;

namespace SPBackend.Queries.GetScheduleDetails;

public class GetScheduleDetailsRequest: IRequest<GetScheduleDetailsResponse>
{
    public long ScheduleId { get; set; }
}