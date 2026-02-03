using MediatR;

namespace SPBackend.Requests.Queries.GetSchedulesByDay;

public class GetSchedulesByDayRequest: IRequest<GetSchedulesByDayResponse>
{
    public DateOnly Date { get; set; }
    public List<long> PlugIds { get; set; } = new();
}