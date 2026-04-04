using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Queries.GetTodayPlugCostSummary;

public class GetTodayPlugCostSummaryRequestHandler : IRequestHandler<GetTodayPlugCostSummaryRequest, GetTodayPlugCostSummaryResponse>
{
    private readonly PlugsService _plugsService;

    public GetTodayPlugCostSummaryRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<GetTodayPlugCostSummaryResponse> Handle(GetTodayPlugCostSummaryRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.GetTodayPlugCostSummary(cancellationToken);
    }
}
