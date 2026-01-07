namespace SPBackend.Models;

public class Policy
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long PolicyTypeId { get; set; }
    public PolicyType PolicyType { get; set; }
    public bool? GreaterThan { get; set; }
    public bool? LessThan { get; set; }
}