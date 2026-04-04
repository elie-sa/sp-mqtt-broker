using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceUsage;

public class GetWeeklyPowerSourceUsageRequestHandler : IRequestHandler<GetWeeklyPowerSourceUsageRequest, GetWeeklyPowerSourceUsageResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetWeeklyPowerSourceUsageRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetWeeklyPowerSourceUsageResponse> Handle(GetWeeklyPowerSourceUsageRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetWeeklyPowerSourceUsage(cancellationToken);
    }
}
