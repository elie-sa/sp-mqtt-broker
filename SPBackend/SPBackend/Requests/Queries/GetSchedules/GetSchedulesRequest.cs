using MediatR;

namespace SPBackend.Requests.Queries.GetSchedules;

public class GetSchedulesRequest: IRequest<GetSchedulesResponse>
{
    public int PageSize { get; set; }
    public int Page { get; set; }
}