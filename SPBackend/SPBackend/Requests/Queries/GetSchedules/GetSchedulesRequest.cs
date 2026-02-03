using MediatR;

namespace SPBackend.Requests.Queries.GetSchedules;

public class GetSchedulesRequest: IRequest<GetSchedulesResponse>
{
    public List<long> PlugIds { get; set; } = new();
}