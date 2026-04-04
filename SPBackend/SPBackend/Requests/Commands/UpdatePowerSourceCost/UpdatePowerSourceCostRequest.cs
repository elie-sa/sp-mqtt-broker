using MediatR;

namespace SPBackend.Requests.Commands.UpdatePowerSourceCost;

public class UpdatePowerSourceCostRequest : IRequest<UpdatePowerSourceCostResponse>
{
    public long PowerSourceId { get; set; }
    public double CostPerKwh { get; set; }
}
