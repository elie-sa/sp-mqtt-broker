using MediatR;

namespace SPBackend.Requests.Commands.AddPolicy;

public class AddPolicyRequest: IRequest<AddPolicyResponse>
{
    public string Name { get; set; }
    public long? PowerSourceId { get; set; }
    public double? TempGreaterThan { get; set; }
    public double? TempLessThan { get; set; }
    public bool IsActive { get; set; }
}