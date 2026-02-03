using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetAllPolicies;

public class GetAllPoliciesResponse
{
    public List<PolicyDto> Policies { get; set; }
}