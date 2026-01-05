namespace SPBackend.Models;

public class Household
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<PowerSource> PowerSources { get; set; }
    public ICollection<Room> Rooms { get; set; }
    public ICollection<User> Users { get; set; }
}