using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetSchedulesByDay;

public class GetSchedulesByDayRequestHandler: IRequestHandler<GetSchedulesByDayRequest, GetSchedulesByDayResponse>
{
    private readonly PlugsService _plugsService;

    public GetSchedulesByDayRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetSchedulesByDayResponse> Handle(GetSchedulesByDayRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetSchedulesByDay(request, cancellationToken);
    }
}