using MediatR;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.TogglePolicy;

public class TogglePolicyRequestHandler :IRequestHandler<TogglePolicyRequest, TogglePolicyResponse>
{
    private readonly PlugsService _plugsService;

    public TogglePolicyRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<TogglePolicyResponse> Handle(TogglePolicyRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.TogglePolicy(request, cancellationToken);
    }
}