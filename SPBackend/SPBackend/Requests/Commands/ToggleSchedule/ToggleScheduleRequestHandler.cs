using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.ToggleSchedule;

public class ToggleScheduleRequestHandler: IRequestHandler<ToggleScheduleRequest, ToggleScheduleResponse>
{
    private readonly PlugsService _plugsService;

    public ToggleScheduleRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<ToggleScheduleResponse> Handle(ToggleScheduleRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.ToggleSchedule(request, cancellationToken);
    }
}