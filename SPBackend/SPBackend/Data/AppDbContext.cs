using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SPBackend.Models;
using SPBackend.Services.Outbox;

namespace SPBackend.Data;

public class AppDbContext: DbContext, IAppDbContext
{
    private bool _suppressOutbox;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Consumption> Consumptions { get; set; }
    public DbSet<RecentConsumption> RecentConsumptions { get; set; }
    public DbSet<DeviceType> DeviceTypes { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<MainsLog> MainsLogs { get; set; }
    public DbSet<Plug> Plugs { get; set; }
    public DbSet<PlugControl> PlugControls { get; set; }
    public DbSet<PowerSource> PowerSources { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PlugPolicy> PlugPolicies { get; set; }
    public DbSet<NotificationToken>  NotificationTokens { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.Property(x => x.EntityType).IsRequired();
            entity.Property(x => x.Operation).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.HasIndex(x => x.ProcessedAtUtc);
            entity.HasIndex(x => x.LastAttemptAtUtc);
        });
    }

    public override int SaveChanges()
    {
        if (_suppressOutbox)
        {
            return base.SaveChanges();
        }

        var pending = CaptureOutboxEntries();
        var result = base.SaveChanges();

        if (pending.Count > 0)
        {
            EnqueueOutboxMessages(pending);
            _suppressOutbox = true;
            try
            {
                base.SaveChanges();
            }
            finally
            {
                _suppressOutbox = false;
            }
        }

        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_suppressOutbox)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        return SaveChangesWithOutboxAsync(cancellationToken);
    }

    private async Task<int> SaveChangesWithOutboxAsync(CancellationToken cancellationToken)
    {
        var pending = CaptureOutboxEntries();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (pending.Count > 0)
        {
            EnqueueOutboxMessages(pending);
            _suppressOutbox = true;
            try
            {
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _suppressOutbox = false;
            }
        }

        return result;
    }

    private List<PendingOutboxEntry> CaptureOutboxEntries()
    {
        ChangeTracker.DetectChanges();

        var entries = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(entry => entry.Entity is not OutboxMessage)
            .Where(entry => !entry.Metadata.IsOwned());

        var trackedEntries = entries.ToList();

        if (trackedEntries.Count == 0)
        {
            return new List<PendingOutboxEntry>();
        }

        var pending = new List<PendingOutboxEntry>(trackedEntries.Count);
        foreach (var entry in trackedEntries)
        {
            var operation = entry.State switch
            {
                EntityState.Added => OutboxOperation.Added,
                EntityState.Modified => OutboxOperation.Modified,
                EntityState.Deleted => OutboxOperation.Deleted,
                _ => throw new InvalidOperationException("Unsupported entity state for outbox.")
            };

            Dictionary<string, object?>? deletedValues = null;
            if (operation == OutboxOperation.Deleted)
            {
                deletedValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsShadowProperty())
                    {
                        continue;
                    }

                    deletedValues[property.Metadata.Name] = property.OriginalValue;
                }
            }

            pending.Add(new PendingOutboxEntry
            {
                Entry = entry,
                Operation = operation,
                DeletedValues = deletedValues
            });
        }

        return pending;
    }

    private void EnqueueOutboxMessages(IReadOnlyList<PendingOutboxEntry> entries)
    {
        if (entries.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var pending in entries)
        {
            var payload = pending.Operation == OutboxOperation.Deleted
                ? new Dictionary<string, object?>(pending.DeletedValues ?? new Dictionary<string, object?>())
                : BuildPayload(pending.Entry);

            OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = now,
                EntityType = pending.Entry.Metadata.Name,
                Operation = pending.Operation.ToString(),
                Payload = System.Text.Json.JsonSerializer.Serialize(payload, OutboxJson.SerializerOptions)
            });
        }
    }

    private static Dictionary<string, object?> BuildPayload(EntityEntry entry)
    {
        var payload = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            payload[property.Metadata.Name] = property.CurrentValue;
        }

        return payload;
    }

    private sealed class PendingOutboxEntry
    {
        public EntityEntry Entry { get; init; } = null!;
        public OutboxOperation Operation { get; init; }
        public Dictionary<string, object?>? DeletedValues { get; init; }
    }
}   
