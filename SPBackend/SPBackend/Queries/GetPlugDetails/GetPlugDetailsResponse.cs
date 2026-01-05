using SPBackend.DTOs;

namespace SPBackend.Queries.GetPlugDetails;

public class GetPlugDetailsResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsConstant { get; set; }
    public bool IsOn { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool IsDeviceConnected { get; set; }
    public double CurrentConsumption { get; set; }
    public List<ScheduleViewModel> Schedules { get; set; } = new();
}