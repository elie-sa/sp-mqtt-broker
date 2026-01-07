using MediatR;

namespace SPBackend.Commands.EditSchedule;

public class EditScheduleRequest: IRequest<EditScheduleResponse>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public List<long> OnPlugIds { get; set; } = new();
    public List<long> OffPlugIds { get; set; } = new();
}