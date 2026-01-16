using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetScheduleDetails;

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