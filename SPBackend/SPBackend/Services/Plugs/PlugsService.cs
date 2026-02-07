using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Models;
using SPBackend.Requests.Commands.AddPolicy;
using SPBackend.Requests.Commands.AddSchedule;
using SPBackend.Requests.Commands.AddTimeout;
using SPBackend.Requests.Commands.DeletePolicy;
using SPBackend.Requests.Commands.DeleteSchedule;
using SPBackend.Requests.Commands.DeleteTimeout;
using SPBackend.Requests.Commands.EditPolicy;
using SPBackend.Requests.Commands.EditSchedule;
using SPBackend.Requests.Commands.RemovePlugFromSchedule;
using SPBackend.Requests.Commands.SetPlug;
using SPBackend.Requests.Commands.SetPlugName;
using SPBackend.Requests.Commands.TogglePolicy;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Requests.Queries.GetAllPlugs;
using SPBackend.Requests.Queries.GetAllPolicies;
using SPBackend.Requests.Queries.GetPlugDetails;
using SPBackend.Requests.Queries.GetPolicy;
using SPBackend.Requests.Queries.GetScheduleDetails;
using SPBackend.Requests.Queries.GetSchedules;
using SPBackend.Requests.Queries.GetSchedulesByDay;
using SPBackend.Requests.Queries.GetSchedulesNextDays;
using SPBackend.Services.CurrentUser;
using SPBackend.Services.Mqtt;
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
        var plug = await _dbContext.Plugs
            .Include(x => x.Consumptions)
            .Include(y => y.PlugControls)
            .ThenInclude(z => z.Schedule)
            .FirstOrDefaultAsync(x => x.Id == plugId, cancellationToken);
        if(plug == null) throw new KeyNotFoundException("No plug was found");

        var recentConsumption = await _dbContext.RecentConsumptions
            .OrderByDescending(x => x.Time)
            .FirstOrDefaultAsync(x => x.PlugId == plugId, cancellationToken);
        var currentConsumptionValue = recentConsumption?.TotalEnergy ?? 0;
        var isDeviceConnected = recentConsumption != null && recentConsumption.Time >= DateTime.UtcNow.AddSeconds(-20);

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

    public async Task<GetSchedulesResponse> GetSchedules(GetSchedulesRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId == _currentUser.Sub, cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var schedules = new List<Schedule>();

        if (request.PlugIds.Count == 0)
        {
            schedules = await _dbContext.Schedules.Where(x => x.Time >= DateTime.UtcNow
                && x.PlugControls.Any(pc => pc.Plug.Room.Household.Users.Any(u => u.Id == user.Id))).ToListAsync(cancellationToken: cancellationToken);
        }
        else
        {
            schedules = await _dbContext.Schedules.Where(x => x.Time >= DateTime.UtcNow && x.PlugControls.Any(x => request.PlugIds.Contains(x.PlugId))
                && x.PlugControls.Any(pc => pc.Plug.Room.Household.Users.Any(u => u.Id == user.Id))).ToListAsync(cancellationToken: cancellationToken);
        }
        
        return new GetSchedulesResponse()
        {
            ScheduledDates = schedules.Select(x => DateOnly.FromDateTime(x.Time)).Distinct().Order().ToList()
        };
    }

    public async Task<GetSchedulesByDayResponse> GetSchedulesByDay(GetSchedulesByDayRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId == _currentUser.Sub, cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var schedules = new List<Schedule>();
        if (request.PlugIds.Count == 0)
        {
            schedules = await _dbContext.Schedules.Include(x => x.PlugControls)
                .Where(x => DateOnly.FromDateTime(x.Time).Equals(request.Date)).ToListAsync(cancellationToken: cancellationToken);
        }
        else
        {
            schedules = await _dbContext.Schedules.Include(x => x.PlugControls)
                .Where(x => DateOnly.FromDateTime(x.Time).Equals(request.Date) && x.PlugControls.Any(pc => request.PlugIds.Contains(pc.PlugId))).ToListAsync(cancellationToken: cancellationToken);
        }
        
        return new GetSchedulesByDayResponse()
        {
            Schedules = schedules.Select(x => new ScheduleDto()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceCount = x.PlugControls.Count,
                Time = x.Time,
                IsActive = x.IsActive
            }).ToList()
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
            Time = schedule.Time,
            IsActive = schedule.IsActive,
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
            IsActive = request.IsActive
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

    public async Task<ToggleScheduleResponse> ToggleSchedule(ToggleScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = _dbContext.Schedules.FirstOrDefault(x => x.Id == request.ScheduleId);
        if(schedule == null) throw new KeyNotFoundException($"No schedule of id {request.ScheduleId} was found");
        
        if(schedule.IsActive == request.Enable) return new ToggleScheduleResponse(){ Message = "Schedule is already " + (schedule.IsActive ? "active." : "inactive.") };
        schedule.IsActive = request.Enable;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ToggleScheduleResponse(){ Message = "Schedule successfully toggled " + (request.Enable ? "active." : "inactive.") };
    }

    public async Task<GetAllPlugsResponse> GetAllPlugs(GetAllPlugsRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken); 
        var plugs = await _dbContext.Plugs.Include(x => x.Room).Where(x => x.Room.Household.Users.Any(u => u.Id == user!.Id)).OrderBy(x => x.Id).ToListAsync(cancellationToken);

        return new GetAllPlugsResponse()
        {
            Plugs = plugs.Select(x => new PlugDto
            {
                Id = x.Id,
                Name = x.Name,
                Room = x.Room.Name
            }).ToList()
        };
    }

    public async Task<AddPolicyResponse> AddPolicy(AddPolicyRequest request, CancellationToken cancellationToken)
    {
        var policyToAdd = new Policy
        {
            Name = request.Name,
            IsActive = request.IsActive
        };

        if (request.PowerSourceId is null && request.TempGreaterThan is null && request.TempLessThan is null)
            throw new ArgumentException("Either a power source or a temperature condition must be provided.");
            
        if(request.PowerSourceId != null) policyToAdd.PowerSourceId = request.PowerSourceId;
        if(request.TempGreaterThan != null) policyToAdd.TempGreaterThan = request.TempGreaterThan;
        if(request.TempLessThan != null) policyToAdd.TempLessThan = request.TempLessThan;
        
        _dbContext.Policies.Add(policyToAdd);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        request.OnPlugIds.ForEach(x => _dbContext.PlugPolicies.Add(new PlugPolicy()
        {
            PlugId = x,
            PolicyId = policyToAdd.Id,
            SetStatus = true
        }));
        
        request.OffPlugIds.ForEach(x => _dbContext.PlugPolicies.Add(new PlugPolicy()
        {
            PlugId = x,
            PolicyId = policyToAdd.Id,
            SetStatus = false
        }));
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddPolicyResponse()
        {
            Message = "Successfully added policy " + policyToAdd.Name
        };
    }

    public async Task<GetAllPoliciesResponse> GetAllPolicies(GetAllPoliciesRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);

        var plugPolicies = new List<PlugPolicy>();
        if (!request.TempOnly && !request.PowerSourceOnly)
        {
            plugPolicies = await _dbContext.PlugPolicies.Include(p => p.Policy)
                .ThenInclude(y => y.PowerSource).Where(x => x.Plug.Room.Household.Users.Any(u => u.Id == user!.Id)).ToListAsync(cancellationToken);
        } else if (request.TempOnly)
        {
            plugPolicies = await _dbContext.PlugPolicies.Include(p => p.Policy)
                .ThenInclude(y => y.PowerSource).Where(x => (x.Policy.TempLessThan != null || x.Policy.TempGreaterThan != null) && x.Plug.Room.Household.Users.Any(u => u.Id == user!.Id)).ToListAsync(cancellationToken);
        }
        else
        {
            plugPolicies = await _dbContext.PlugPolicies.Include(p => p.Policy)
                .ThenInclude(y => y.PowerSource).Where(x => x.Policy.PowerSource != null && x.Plug.Room.Household.Users.Any(u => u.Id == user!.Id)).ToListAsync(cancellationToken);
        }
        
        if (plugPolicies == null) throw new Exception("No policies found.");

        return new GetAllPoliciesResponse()
        {
            Policies = plugPolicies
                .GroupBy(pp => pp.Policy.Id)
                .Select(g =>
                {
                    var policy = g.First().Policy;

                    return new PolicyDto
                    {
                        Id = policy.Id,
                        Name = policy.Name,
                        PowerSourceId = policy.PowerSourceId,
                        PowerSourceName = (policy.PowerSource == null) ? null : policy.PowerSource!.Name,
                        IsActive = policy.IsActive,
                        TempGreaterThan = policy.TempGreaterThan,
                        TempLessThan = policy.TempGreaterThan, 
                        NumOfPlugs = g.Count()
                    };
                }).ToList()
        };
    }

    public async Task<GetPolicyResponse> GetPolicy(long requestPolicyId, CancellationToken cancellationToken)
    {
        var policy = await _dbContext.Policies.Include(x => x.PowerSource).Where(x => x.Id == requestPolicyId).FirstOrDefaultAsync(cancellationToken);
        if(policy is null) throw new ArgumentException("No policy found.");
        var plugPolicies = await _dbContext.PlugPolicies.Where(x => x.PolicyId == requestPolicyId && x.SetStatus == true)
            .Include(x => x.Plug).ToListAsync(cancellationToken);   
        if(plugPolicies == null) throw new Exception("No plug policies found.");
        
        var output = new GetPolicyResponse()
        {
            Id = policy.Id,
            Name = policy.Name,
            IsActive = policy.IsActive,
            TempGreaterThan = policy.TempGreaterThan,
            TempLessThan = policy.TempLessThan,
            PowerSourceId = policy.PowerSourceId,
            PowerSourceName = (policy.PowerSource == null) ? null : policy.PowerSource!.Name,
            NumOfPlugs = plugPolicies.Count(),
            OnPlugs = (
                plugPolicies.Where(x => x.SetStatus).Select(x => new SmallPlugDto()
            {
                Id = x.PlugId,
                Name = x.Plug.Name,
            }).ToList()),
            OffPlugs = (
                plugPolicies.Where(x => !x.SetStatus).Select(x => new SmallPlugDto()
                {
                    Id = x.PlugId,
                    Name = x.Plug.Name,
                }).ToList())
        };

        return output;
    }

    public async Task<GetSchedulesNextDaysResponse> GetSchedulesNextDays(GetSchedulesNextDaysRequest request, CancellationToken cancellationToken)
    {
         var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.KeyCloakId == _currentUser.Sub, cancellationToken);
        
        if (user == null) throw new ArgumentException("User not found");
        
        var today = DateOnly.FromDateTime(DateTime.Now);

        var query = new List<Schedule>();

        if (request.PlugId == null)
        {
            query = await _dbContext.Schedules
                .AsNoTracking()
                .Include(s => s.PlugControls)
                .Where(s => DateOnly.FromDateTime(s.Time) >= today).Distinct().ToListAsync(cancellationToken);
        }
        else
        {
            query = await _dbContext.Schedules
                .AsNoTracking()
                .Include(s => s.PlugControls)
                .Where(s => DateOnly.FromDateTime(s.Time) >= today && s.PlugControls.Any(pc => pc.PlugId == request.PlugId)).Distinct().ToListAsync(cancellationToken);
        }
        
        var nextDates = query
            .Select(s => DateOnly.FromDateTime(s.Time))
            .Distinct()
            .OrderBy(d => d)
            .Take(2);
    
        if (!nextDates.Any())
        {
            return new GetSchedulesNextDaysResponse { Days = new List<DaySchedulesDto>() };
        }

        var schedules = query
            .Where(s => nextDates.Contains(DateOnly.FromDateTime(s.Time)));
    
        var days = schedules
            .GroupBy(s => DateOnly.FromDateTime(s.Time))
            .OrderBy(g => g.Key)
            .Select(g => new DaySchedulesDto
            {
                Date = g.Key,
                Schedules = g.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DeviceCount = s.PlugControls.Count,
                    Time = s.Time,
                    IsActive = s.IsActive
                }).ToList()
            })
            .ToList();
    
        return new GetSchedulesNextDaysResponse { Days = days };
    }

    public async Task<DeletePolicyResponse> DeletePolicy(long requestPolicyId, CancellationToken cancellationToken)
    {
        var policy = await _dbContext.Policies.FirstOrDefaultAsync(x => x.Id == requestPolicyId, cancellationToken);
        if (policy == null) throw new KeyNotFoundException($"No policy of id {requestPolicyId} was found");
        
        _dbContext.Policies.Remove(policy);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new DeletePolicyResponse(){ Message = "Successfully deleted policy " + policy.Name };
    }

    public async Task<EditPolicyResponse> EditPolicy(EditPolicyRequest request, CancellationToken cancellationToken)
    {
        var policy = await _dbContext.Policies
            .Include(p => p.PlugPolicies)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (policy == null)
            throw new KeyNotFoundException($"No policy of id {request.Id} was found");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.KeyCloakId == _currentUser.Sub, cancellationToken);

        if (user == null)
            throw new ArgumentException("User not found");

        var otherPolicies = await _dbContext.Policies
            .Where(p =>
                p.PlugPolicies.Any(pp =>
                    pp.Plug.Room.Household.Users.Any(u => u.Id == user.Id)
                ) && p.Id != policy.Id)
            .ToListAsync(cancellationToken);

        if (otherPolicies.Any(p => p.Name == request.Name))
            throw new ArgumentException($"There already exists a policy with name {request.Name}.");

        policy.Name = request.Name;
        var powerSource = _dbContext.PowerSources.FirstOrDefault(x => x.Id == policy.PowerSourceId);
        if (powerSource == null) throw new ArgumentException("Invalid power source id provided");
        policy.PowerSourceId = request.PowerSourceId;
        policy.TempGreaterThan = request.TempGreaterThan;
        policy.TempLessThan = request.TempLessThan;

        _dbContext.PlugPolicies.RemoveRange(policy.PlugPolicies);

        var plugPoliciesToAdd =
            request.OnPlugIds.Select(id => new PlugPolicy
                {
                    PlugId = id,
                    PolicyId = policy.Id,
                    SetStatus = true
                })
                .Concat(request.OffPlugIds.Select(id => new PlugPolicy
                {
                    PlugId = id,
                    PolicyId = policy.Id,
                    SetStatus = false
                }))
                .ToList();

        policy.PlugPolicies = plugPoliciesToAdd;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EditPolicyResponse
        {
            Message = "Successfully edited policy " + policy.Name
        };
    }

    public async Task<TogglePolicyResponse> TogglePolicy(TogglePolicyRequest request, CancellationToken cancellationToken)
    {
        var policy = await _dbContext.Policies
            .FirstOrDefaultAsync(x => x.Id == request.PolicyId, cancellationToken);

        if (policy == null)
            throw new KeyNotFoundException($"No policy of id {request.PolicyId} was found");

        if (policy.IsActive == request.Enable)
        {
            return new TogglePolicyResponse
            {
                Message = "Policy is already " + (policy.IsActive ? "active." : "inactive.")
            };
        }

        policy.IsActive = request.Enable;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TogglePolicyResponse
        {
            Message = "Policy successfully toggled " + (request.Enable ? "active." : "inactive.")
        };
    }
}
