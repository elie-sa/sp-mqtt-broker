using System.Data.SqlTypes;

namespace SPBackend.Models;

public class PowerSource
{
    public long Id { get; set; }
    public string Name { get; set; }
    public double MaxCapacity { get; set; }
    public long HouseholdId { get; set; }
    public Household Household { get; set; }
    public ICollection<MainsLog> Mains { get; set; }
}