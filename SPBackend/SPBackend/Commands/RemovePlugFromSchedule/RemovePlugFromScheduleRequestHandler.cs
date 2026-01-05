using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Commands.RemovePlugFromSchedule;

public class RemovePlugFromScheduleRequestHandler: IRequestHandler<RemovePlugFromScheduleRequest, RemovePlugFromScheduleResponse>
{
    private readonly PlugsService _plugsService;

    public RemovePlugFromScheduleRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<RemovePlugFromScheduleResponse> Handle(RemovePlugFromScheduleRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.RemovePlugFromSchedule(request, cancellationToken);
    }
}