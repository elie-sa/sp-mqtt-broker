namespace SPBackend.Queries.GetScheduleDetails;

public class GetScheduleDetailsResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public List<ScheduleDeviceDto> OnPlugs { get; set; } = new();
    public List<ScheduleDeviceDto> OffPlugs { get; set; } = new();
}

public class ScheduleDeviceDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }
}