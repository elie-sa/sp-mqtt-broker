using MediatR;

namespace SPBackend.Requests.Queries.GetPlugDetails;

public class GetPlugDetailsRequest: IRequest<GetPlugDetailsResponse>
{
    public long PlugId { get; set; }
}