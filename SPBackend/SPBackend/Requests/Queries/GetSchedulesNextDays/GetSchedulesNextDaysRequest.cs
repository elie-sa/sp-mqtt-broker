using MediatR;

namespace SPBackend.Requests.Queries.GetSchedulesNextDays;

public class GetSchedulesNextDaysRequest: IRequest<GetSchedulesNextDaysResponse>
{
    public long? PlugId { get; set; }
}
