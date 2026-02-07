using Microsoft.EntityFrameworkCore;
using SPBackend.Models;

namespace SPBackend.Data;

public interface IAppDbContext
{
    public DbSet<Consumption> Consumptions { get; set; }
    public DbSet<RecentConsumption> RecentConsumptions { get; set; }
    public DbSet<DeviceType> DeviceTypes { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<MainsLog> MainsLogs { get; set; }
    public DbSet<Plug> Plugs { get; set; }
    public DbSet<PlugControl> PlugControls { get; set; }
    public DbSet<PowerSource> PowerSources { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PlugPolicy> PlugPolicies { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
