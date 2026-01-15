using Microsoft.EntityFrameworkCore;
using SPBackend.Commands.AddSchedule;
using SPBackend.Commands.AddTimeout;
using SPBackend.Commands.DeleteSchedule;
using SPBackend.Commands.DeleteTimeout;
using SPBackend.Commands.EditSchedule;
using SPBackend.Commands.RemovePlugFromSchedule;
using SPBackend.Commands.SetPlug;
using SPBackend.Commands.SetPlugName;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Models;
using SPBackend.Queries.GetPlugDetails;
using SPBackend.Queries.GetScheduleDetails;
using SPBackend.Queries.GetSchedules;
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
        //TODO: Add the constant plug logic
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
        if (schedule == null) throw new KeyNotFoundException("No schedule was found");
        
        var plug = await _dbContext.Plugs.Include(x => x.PlugControls).FirstOrDefaultAsync(x => x.Id == request.PlugId, cancellationToken);
        if (plug == null) throw new KeyNotFoundException("No plugs was found");
        
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

    public async Task<AddTimeoutResponse> AddTimeout(AddTimeoutRequest request, CancellationToken cancellationToken)
    {
        var plug = _dbContext.Plugs.FirstOrDefault(x => x.Id.Equals(request.PlugId));
        if (plug == null) throw new KeyNotFoundException("No plug was found");
        
        plug.Timeout = request.Timeout;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddTimeoutResponse() { Message = $"Successfully added timeout of {plug.Timeout} to plug {plug.Id}" };
    }
    
    public async Task<DeleteTimeoutResponse> DeleteTimeout(long plugId, CancellationToken cancellationToken)
    {
        var plug = _dbContext.Plugs.FirstOrDefault(x => x.Id.Equals(plugId));
        if (plug == null) throw new KeyNotFoundException("No plug was found");
        
        plug.Timeout = null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteTimeoutResponse() { Message = $"Successfully added timeout of {plug.Timeout} to plug {plug.Id}" };
    }

    public async Task<GetSchedulesResponse> GetSchedules(CancellationToken cancellationToken, int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        if (pageSize > 100) pageSize = 100;
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken); 

        var query =
            _dbContext.Schedules
                .AsNoTracking()
                .Where(s =>
                    s.PlugControls.Any(pc =>
                        pc.Plug.Room.Household.Users.Any(u => u.Id == user.Id)
                    ))
                .OrderBy(s => s.Time);

        var totalCount = await query.CountAsync(cancellationToken);

        var schedules = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ScheduleDto()
            {
                Id = s.Id,
                Name = s.Name,
                Time = s.Time,
                DeviceCount = s.PlugControls.Count()
            })
            .ToListAsync(cancellationToken);

        return new GetSchedulesResponse()
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Schedules = schedules
        };
    }

    public async Task<GetScheduleDetailsResponse> GetScheduleDetails(GetScheduleDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.Schedules.Include(x => x.PlugControls).ThenInclude(y => y.Plug).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, cancellationToken);
        if (schedule == null) throw new KeyNotFoundException("No schedule was found");

        var onPlugs = schedule.PlugControls.Where(x => x.SetStatus == true).ToList();
        var offPlugs = schedule.PlugControls.Where(x => x.SetStatus == false).ToList();
        
        var response = new GetScheduleDetailsResponse()
        {
            Id = schedule.Id,
            Name = schedule.Name,
            Time = schedule.Time
        };
        
        response.OnPlugs.AddRange(onPlugs.Select(x => new SchedulePlugDto()
        {
            Id = x.PlugId,
            Name = x.Plug.Name,
            IsOn = x.Plug.IsOn
        }));
        
        response.OffPlugs.AddRange(offPlugs.Select(x => new SchedulePlugDto()
        {
            Id = x.PlugId,
            Name = x.Plug.Name,
            IsOn = x.Plug.IsOn
        }));
        
        return response;
    }

    public async Task<AddScheduleResponse> AddSchedule(AddScheduleRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken); 
        
        var query =
            _dbContext.Schedules
                .AsNoTracking()
                .Where(s =>
                    s.PlugControls.Any(pc =>
                        pc.Plug.Room.Household.Users.Any(u => u.Id == user.Id)
                    ))
                .OrderBy(s => s.Time).ToList();

        if (query.Any(x => x.Time.Equals(request.Time)))
            throw new ArgumentException($"There already exists a schedule at time {request.Time}.");
        if (query.Any(x => x.Name.Equals(request.Name)))
            throw new ArgumentException($"There already exists a schedule with name {request.Name}.");
        
        var scheduleToAdd = new Schedule()
        {
            Name = request.Name,
            Time = request.Time,
        };

        if (query.Any(x => x.Time.Equals(request.Time))) ;
        
        await _dbContext.Schedules.AddAsync(scheduleToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    
        var plugControlsToAdd = new List<PlugControl>();
        
        request.OnPlugIds.ForEach(x => plugControlsToAdd.Add(new PlugControl()
        {
            PlugId = x,
            ScheduleId = scheduleToAdd.Id,
            SetStatus = true
        }));
        
        request.OffPlugIds.ForEach(x => plugControlsToAdd.Add(new PlugControl()
        {
            PlugId = x,
            ScheduleId = scheduleToAdd.Id,
            SetStatus = false
        }));
        
        await _dbContext.PlugControls.AddRangeAsync(plugControlsToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AddScheduleResponse(){ Message = "Successfully added schedule " + scheduleToAdd.Name };
    }

    public async Task<DeleteScheduleResponse> DeleteSchedule(long scheduleId,
        CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(x => x.Id == scheduleId, cancellationToken);
        if (schedule == null) throw new KeyNotFoundException($"No schedule of id {scheduleId} was found");
        
        _dbContext.Schedules.Remove(schedule);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteScheduleResponse(){ Message = "Successfully deleted schedule " + schedule.Name };
    }

    public async Task<EditScheduleResponse> EditSchedule(EditScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.Schedules.Include(x => x.PlugControls).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (schedule == null) throw new KeyNotFoundException($"No schedule of id {request.Id} was found");
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken); 
        
        var query =
            _dbContext.Schedules
                .Where(s =>
                    s.PlugControls.Any(pc =>
                        pc.Plug.Room.Household.Users.Any(u => u.Id == user.Id) 
                    ) && s.Id != schedule.Id)
                .OrderBy(s => s.Time).ToList();
        
        if (query.Any(x => x.Time.Equals(request.Time)))
            throw new ArgumentException($"There already exists a schedule at time {request.Time}.");
        if (query.Any(x => x.Name.Equals(request.Name)))
            throw new ArgumentException($"There already exists a schedule with name {request.Name}.");
        
        schedule.Name = request.Name;
        schedule.Time = request.Time;
        
        _dbContext.PlugControls.RemoveRange(schedule.PlugControls);

        var plugControlsToAdd =
            request.OnPlugIds.Select(id => new PlugControl
                {
                    PlugId = id,
                    ScheduleId = schedule.Id,
                    SetStatus = true
                })
                .Concat(request.OffPlugIds.Select(id => new PlugControl
                {
                    PlugId = id,
                    ScheduleId = schedule.Id,
                    SetStatus = false
                }))
                .ToList();

        schedule.PlugControls = plugControlsToAdd;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EditScheduleResponse { Message = "Successfully edited schedule " + schedule.Name };
    }
}