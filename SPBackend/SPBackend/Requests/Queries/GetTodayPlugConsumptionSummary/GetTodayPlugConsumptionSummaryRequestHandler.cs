using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetTodayPlugConsumptionSummary;

public class GetTodayPlugConsumptionSummaryRequestHandler : IRequestHandler<GetTodayPlugConsumptionSummaryRequest, GetTodayPlugConsumptionSummaryResponse>
{
    private readonly PlugsService _plugsService;

    public GetTodayPlugConsumptionSummaryRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetTodayPlugConsumptionSummaryResponse> Handle(GetTodayPlugConsumptionSummaryRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetTodayPlugConsumptionSummary(cancellationToken);
    }
}
