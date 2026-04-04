using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Commands.UpdatePowerSourceCost;

public class UpdatePowerSourceCostRequestHandler : IRequestHandler<UpdatePowerSourceCostRequest, UpdatePowerSourceCostResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public UpdatePowerSourceCostRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public async Task<UpdatePowerSourceCostResponse> Handle(UpdatePowerSourceCostRequest request, CancellationToken cancellationToken)
    {
        return await _powerSourceService.UpdatePowerSourceCost(request, cancellationToken);
    }
}
