using MediatR;
using SPBackend.Queries.GetSchedules;
using SPBackend.Services.Plugs;

namespace SPBackend.Queries.GetScheduleDetails;

public class GetScheduleDetailsRequestHandler: IRequestHandler<GetScheduleDetailsRequest, GetScheduleDetailsResponse>
{
    private readonly PlugsService _plugsService;

    public GetScheduleDetailsRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetScheduleDetailsResponse> Handle(GetScheduleDetailsRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetScheduleDetails(request, cancellationToken);
    }
}