namespace SPBackend.Requests.Queries.GetPowerSource;

public class GetPowerSourceResponse
{
    public long PowerSourceId { get; set; }
    public string Name { get; set; }
    public long Voltage { get; set; }
    public DateTime LastUpdated { get; set; }
}