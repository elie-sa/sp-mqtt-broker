using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Models;
using SPBackend.Services.Mqtt;

namespace SPBackend.Services.Plugs;

public class ScheduleJobService
{
    private readonly IAppDbContext _dbContext;
    private readonly MqttService _mqttService;

    public ScheduleJobService(IAppDbContext dbContext, MqttService mqttService)
    {
        _dbContext = dbContext;
        _mqttService = mqttService;
    }

    public async Task ExecuteSchedule(long scheduleId)
    {
        var schedule = await _dbContext.Schedules
            .Include(s => s.PlugControls)
                .ThenInclude(pc => pc.Plug)
                    .ThenInclude(p => p.Room)
            .FirstOrDefaultAsync(s => s.Id == scheduleId);

        if (schedule == null || !schedule.IsActive)
        {
            return;
        }

        if (schedule.PlugControls.Count == 0)
        {
            return;
        }

        var hasChanges = false;
        var shouldConnect = schedule.PlugControls.Any(pc => pc.Plug.IsOn != pc.SetStatus);
        if (shouldConnect)
        {
            await _mqttService.ConnectAsync();
        }

        foreach (var plugControl in schedule.PlugControls)
        {
            var plug = plugControl.Plug;

            if (plug.IsOn == plugControl.SetStatus)
            {
                continue;
            }

            if (plugControl.SetStatus && plug.IsConstant)
            {
                var recentMainsLog = await _dbContext.MainsLogs
                    .Include(x => x.PowerSource)
                    .Where(x => x.HouseholdId == plug.Room.HouseholdId && x.Time <= DateTime.UtcNow.AddSeconds(-20))
                    .OrderBy(x => x.Time)
                    .FirstOrDefaultAsync();

                if (recentMainsLog == null)
                {
                    continue;
                }

                var sumOfConsumptions = await _dbContext.Consumptions
                    .Where(x => x.Plug.Room.HouseholdId == plug.Room.HouseholdId)
                    .SumAsync(x => x.TotalEnergy);

                if (recentMainsLog.PowerSource.MaxCapacity <= 1.1 * (sumOfConsumptions + plug.ConstantConsumption))
                {
                    continue;
                }
            }

            await _mqttService.PublishAsync($"home/plug/{plug.Id}", plugControl.SetStatus ? "ON" : "OFF");
            plug.IsOn = plugControl.SetStatus;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
