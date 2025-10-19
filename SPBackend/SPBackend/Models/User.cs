namespace SPBackend.Models;

public class User
{
    public long Id { get; set; }
    public string KeyCloakId { get; set; }
    public long HouseholdId { get; set; }
    public Household Household { get; set; }
}