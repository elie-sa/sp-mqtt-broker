using Microsoft.EntityFrameworkCore;
using SPBackend.Commands.RemovePlugFromSchedule;
using SPBackend.Commands.SetPlug;
using SPBackend.Commands.SetPlugName;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Queries.GetPlugDetails;
using SPBackend.Services.CurrentUser;
using SPBackend.Services.MQTTService;

namespace SPBackend.Services.Plugs;

public class PlugsService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly MqttService _mqttService;

    public PlugsService(IAppDbContext dbContext, ICurrentUser currentUser, MqttService mqttService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mqttService = mqttService;
    }

    public async Task<GetPlugDetailsResponse> GetPlugDetails(long plugId,
        CancellationToken cancellationToken)
    {
        var plug = await _dbContext.Plugs.Include(x => x.Consumptions).Include(y => y.PlugControls).ThenInclude(z => z.Schedule).FirstOrDefaultAsync(x => x.Id == plugId, cancellationToken);
        if(plug == null) throw new KeyNotFoundException("No plug was found");

        var currentConsumption = plug.Consumptions.OrderBy(x => x.Time).FirstOrDefault(x => x.Time >= DateTime.Now.AddSeconds(-20));
        var currentConsumptionValue = currentConsumption?.TotalEnergy ?? 0;
        var isDeviceConnected = currentConsumption != null;

        var schedules = new List<ScheduleViewModel>();
        foreach (var plugControl in plug.PlugControls)
        {
            schedules.Add(new ScheduleViewModel()
            {
                Id = plugControl.Schedule.Id,
                Name = plugControl.Schedule.Name,
                Time = plugControl.Schedule.Time,
                SetStatus = plugControl.SetStatus
            });
        }
        
        var response = new GetPlugDetailsResponse()
        {
            Id = plug.Id,
            Name = plug.Name,
            IsConstant = plug.IsConstant,
            IsOn = plug.IsOn,
            Timeout = plug.Timeout,
            IsDeviceConnected = isDeviceConnected,
            CurrentConsumption = currentConsumptionValue,
            Schedules = schedules
        };
        
        return response;
    }

    public async Task<SetPlugResponse> SetPlug(SetPlugRequest request, CancellationToken cancellationToken)
    {
        //TODO: Should i add an ack??
        var plug = _dbContext.Plugs.FirstOrDefault(x => x.Id.Equals(request.PlugId));
        if (plug == null) throw new KeyNotFoundException("No plug was found");
        if (plug.IsOn.Equals(request.SwitchOn)) return new SetPlugResponse(){ Message = request.SwitchOn ? "Plug was already on." : "Plug was already off." };
        
        await _mqttService.ConnectAsync();
        await _mqttService.PublishAsync($"home/plug/{request.PlugId}", request.SwitchOn ? "\"on\": true" : "\"on\": false");
        plug.IsOn = request.SwitchOn;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SetPlugResponse(){ Message = request.SwitchOn ? "Plug switched on." : "Plug switched off." };
    }

    public async Task<RemovePlugFromScheduleResponse> RemovePlugFromSchedule(RemovePlugFromScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(x => x.Id == request.ScheduleId, cancellationToken);
        if (schedule == null) throw new KeyNotFoundException("No plug was found");
        
        var plug = await _dbContext.Plugs.Include(x => x.PlugControls).FirstOrDefaultAsync(x => x.Id == request.PlugId, cancellationToken);
        if (plug == null) throw new KeyNotFoundException("No plug was found");
        
        var plugControl = plug.PlugControls.FirstOrDefault(x => x.ScheduleId == schedule.Id);
        if(plugControl == null) throw new KeyNotFoundException("No plug was found");
        
        plug.PlugControls.Remove(plugControl);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RemovePlugFromScheduleResponse()
            { Message = $"Plug successfully removed from schedule {schedule.Name}." };
    }

    public async Task<SetPlugNameResponse> SetPlugName(SetPlugNameRequest request, CancellationToken cancellationToken)
    {
        var plug = _dbContext.Plugs.FirstOrDefault(x => x.Id.Equals(request.PlugId));
        if (plug == null) throw new KeyNotFoundException("No plug was found");
        
        plug.Name = request.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SetPlugNameResponse(){ Message = $"Plug name successfully changed to ${request.Name}" };
    }
    
}