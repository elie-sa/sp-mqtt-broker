using System.Diagnostics;
using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Commands.AddSchedule;

public class AddScheduleRequestHandler: IRequestHandler<AddScheduleRequest, AddScheduleResponse>
{
    private readonly PlugsService _plugsService;

    public AddScheduleRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<AddScheduleResponse> Handle(AddScheduleRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.AddSchedule(request, cancellationToken);
    }
}