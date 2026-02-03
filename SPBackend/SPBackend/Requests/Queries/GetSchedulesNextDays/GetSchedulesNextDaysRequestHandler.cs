using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetSchedulesNextDays;

public class GetSchedulesNextDaysRequestHandler: IRequestHandler<GetSchedulesNextDaysRequest, GetSchedulesNextDaysResponse>
{
    private readonly PlugsService _plugsService;

    public GetSchedulesNextDaysRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetSchedulesNextDaysResponse> Handle(GetSchedulesNextDaysRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetSchedulesNextDays(request, cancellationToken);
    }
}