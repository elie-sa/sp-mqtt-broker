using MediatR;
using SPBackend.Requests.Commands.DeletePolicy;

namespace SPBackend.Requests.Commands.EditPolicy;

public class EditPolicyRequest: IRequest<EditPolicyResponse>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? PowerSourceId { get; set; }
    public double? TempGreaterThan { get; set; }
    public double? TempLessThan { get; set; }
    public List<long> OnPlugIds { get; set; } = new();
    public List<long> OffPlugIds { get; set; } = new();
}