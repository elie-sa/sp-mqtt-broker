using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceCosts;

public class GetWeeklyPowerSourceCostsRequestHandler : IRequestHandler<GetWeeklyPowerSourceCostsRequest, GetWeeklyPowerSourceCostsResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetWeeklyPowerSourceCostsRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetWeeklyPowerSourceCostsResponse> Handle(GetWeeklyPowerSourceCostsRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetWeeklyPowerSourceCosts(cancellationToken);
    }
}
