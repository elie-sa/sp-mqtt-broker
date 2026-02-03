using MediatR;

namespace SPBackend.Requests.Queries.GetPolicy;

public class GetPolicyRequest: IRequest<GetPolicyResponse>
{
    public long PolicyId { get; set; }
}