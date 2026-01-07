using MediatR;
using SPBackend.DTOs;

namespace SPBackend.Commands.AddSchedule;

public class AddScheduleRequest: IRequest<AddScheduleResponse>
{
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public List<long> OnPlugIds { get; set; } = new();
    public List<long> OffPlugIds { get; set; } = new();
}