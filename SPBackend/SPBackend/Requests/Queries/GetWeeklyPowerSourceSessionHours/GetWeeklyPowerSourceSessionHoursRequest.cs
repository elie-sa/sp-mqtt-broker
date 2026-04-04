using MediatR;

namespace SPBackend.Requests.Queries.GetWeeklyPowerSourceSessionHours;

public class GetWeeklyPowerSourceSessionHoursRequest : IRequest<GetWeeklyPowerSourceSessionHoursResponse>
{
    public long PowerSourceId { get; set; }
}
