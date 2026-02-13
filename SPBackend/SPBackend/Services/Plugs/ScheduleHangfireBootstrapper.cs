using Hangfire;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;

namespace SPBackend.Services.Plugs;

public class ScheduleHangfireBootstrapper : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduleHangfireBootstrapper(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        var schedules = await dbContext.Schedules
            .Where(s => s.IsActive && s.Time > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            schedule.HangfireJobId = backgroundJobClient.Schedule<ScheduleJobService>(
                job => job.ExecuteSchedule(schedule.Id),
                schedule.Time);
        }

        if (schedules.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
