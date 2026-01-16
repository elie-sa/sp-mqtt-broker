using MediatR;

namespace SPBackend.Requests.Queries.GetScheduleDetails;

public class GetScheduleDetailsRequest: IRequest<GetScheduleDetailsResponse>
{
    public long ScheduleId { get; set; }
}