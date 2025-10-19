namespace SPBackend.Models;

public class DeviceType
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<Plug> Plugs { get; set; }
}