using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.DeleteSchedule;

public class DeleteScheduleRequestHandler: IRequestHandler<DeleteScheduleRequest, DeleteScheduleResponse>
{
    private readonly PlugsService _plugsService;

    public DeleteScheduleRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<DeleteScheduleResponse> Handle(DeleteScheduleRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.DeleteSchedule(request.ScheduleId, cancellationToken);
    }
}