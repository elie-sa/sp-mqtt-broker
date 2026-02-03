using MediatR;
using SPBackend.Requests.Commands.ToggleSchedule;

namespace SPBackend.Requests.Commands.TogglePolicy;

public class TogglePolicyRequest : IRequest<TogglePolicyResponse>
{
    public long PolicyId { get; set; }
    public bool Enable { get; set; }
}