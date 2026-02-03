using SPBackend.Requests.Queries.GetAllPlugs;

namespace SPBackend.Requests.Queries.GetPolicy;

public class GetPolicyResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? PowerSourceId { get; set; }
    public string? PowerSourceName { get; set; }
    public double? TempGreaterThan { get; set; }
    public double? TempLessThan { get; set; }
    public long NumOfPlugs { get; set; }
    public bool IsActive { get; set; }
    public List<SmallPlugDto> Plugs { get; set; }
}

public class SmallPlugDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}