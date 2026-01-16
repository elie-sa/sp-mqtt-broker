using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.EditSchedule;

public class EditScheduleRequestHandler: IRequestHandler<EditScheduleRequest, EditScheduleResponse>
{
    private readonly PlugsService _plugsService;

    public EditScheduleRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<EditScheduleResponse> Handle(EditScheduleRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.EditSchedule(request, cancellationToken);
    }
}