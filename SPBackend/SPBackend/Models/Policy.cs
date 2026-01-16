namespace SPBackend.Models;

public class Policy
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? PowerSourceId { get; set; }
    public PowerSource? PowerSource { get; set; }
    public double? TempGreaterThan { get; set; }
    public double? TempLessThan { get; set; }
}