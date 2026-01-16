namespace SPBackend.Requests.Queries.GetScheduleDetails;

public class GetScheduleDetailsResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public bool IsActive { get; set; }
    public List<SchedulePlugDto> OnPlugs { get; set; } = new();
    public List<SchedulePlugDto> OffPlugs { get; set; } = new();
}

public class SchedulePlugDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }
}