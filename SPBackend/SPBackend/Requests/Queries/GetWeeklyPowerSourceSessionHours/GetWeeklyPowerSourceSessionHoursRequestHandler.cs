using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceSessionHours;

public class GetWeeklyPowerSourceSessionHoursRequestHandler : IRequestHandler<GetWeeklyPowerSourceSessionHoursRequest, GetWeeklyPowerSourceSessionHoursResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetWeeklyPowerSourceSessionHoursRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<GetWeeklyPowerSourceSessionHoursResponse> Handle(GetWeeklyPowerSourceSessionHoursRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.GetWeeklyPowerSourceSessionHours(request.PowerSourceId, cancellationToken);
    }
}
