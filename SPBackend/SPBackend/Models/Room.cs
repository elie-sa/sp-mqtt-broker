using System.Security.Cryptography.X509Certificates;

namespace SPBackend.Models;

public class Room
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long RoomTypeId { get; set; }
    public RoomType RoomType { get; set; }
    public long HouseholdId { get; set; }
    public Household Household { get; set; }
    public ICollection<Plug> Plugs { get; set; }
}