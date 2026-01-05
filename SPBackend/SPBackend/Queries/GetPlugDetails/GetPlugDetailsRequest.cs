using MediatR;

namespace SPBackend.Queries.GetPlugDetails;

public class GetPlugDetailsRequest: IRequest<GetPlugDetailsResponse>
{
    public long PlugId { get; set; }
}