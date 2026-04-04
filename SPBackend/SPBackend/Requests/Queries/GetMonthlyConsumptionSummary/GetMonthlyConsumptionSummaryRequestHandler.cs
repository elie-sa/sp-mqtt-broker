using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;

public class GetMonthlyConsumptionSummaryRequestHandler : IRequestHandler<GetMonthlyConsumptionSummaryRequest, GetMonthlyConsumptionSummaryResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetMonthlyConsumptionSummaryRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetMonthlyConsumptionSummaryResponse> Handle(GetMonthlyConsumptionSummaryRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetMonthlyConsumptionSummary(cancellationToken);
    }
}
