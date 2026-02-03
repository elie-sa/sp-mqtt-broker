using SPBackend.Models;

namespace SPBackend.DTOs;

public class PolicyDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? PowerSourceId { get; set; }
    public string? PowerSourceName { get; set; }
    public double? TempGreaterThan { get; set; }
    public double? TempLessThan { get; set; }
    public long NumOfPlugs { get; set; }
    public bool IsActive { get; set; }
}