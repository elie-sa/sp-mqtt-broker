using MediatR;

namespace SPBackend.Requests.Queries.GetSchedulesOfPlug;

public class GetSchedulesOfPlugRequest: IRequest<GetSchedulesOfPlugResponse>
{
    public long PlugId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}