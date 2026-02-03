using MediatR;

namespace SPBackend.Requests.Queries.GetAllPolicies;

public class GetAllPoliciesRequest: IRequest<GetAllPoliciesResponse>
{
    public bool PowerSourceOnly { get; set; }
    public bool TempOnly { get; set; }
}