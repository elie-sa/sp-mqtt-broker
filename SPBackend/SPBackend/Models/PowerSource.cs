namespace SPBackend.Models;

public class PowerSource
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long MaxCapacity { get; set; }
    public ICollection<Mains> Mains { get; set; }
}