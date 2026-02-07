using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Requests.Queries.GetAllSources;
using SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPlugsPerRoomOverview;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Services.CurrentUser;

namespace SPBackend.Services.Mains;

public class PowerSourceService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PowerSourceService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<GetPowerSourceResponse> GetPowerSource()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub));
        var mainsLog = await _dbContext.MainsLogs.Include(x => x.PowerSource).OrderByDescending(p => p.Time).FirstOrDefaultAsync(x => x.HouseholdId.Equals(user.HouseholdId));
        if (mainsLog == null)
        {
            throw new KeyNotFoundException("The household id provided is invalid or no logs have been recorded yet.");
        }
        
        return new GetPowerSourceResponse()
        {
            PowerSourceId = mainsLog.PowerSource.Id,
            Name = mainsLog.PowerSource.Name,
            Voltage = mainsLog.Voltage,
            LastUpdated = mainsLog.Time,
        };
    }

    public async Task<GetGroupedPerDayRoomConsumptionResponse> GetGroupedPerDayRoomConsumption()
    {
       var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub)); 
       var groupedPerDayRoomConsumption = new GetGroupedPerDayRoomConsumptionResponse(){ GroupedRooms = new List<GroupedRoomConsumption>() };
       var roomsPerRoomTypes = await _dbContext.Rooms.Include(x => x.RoomType).Include(x => x.Plugs).ThenInclude(p => p.Consumptions)
           .Where(x => x.HouseholdId == user!.HouseholdId).GroupBy(x => x.RoomType.Name).ToListAsync();

       foreach (var roomPerRoomType in roomsPerRoomTypes)
       {
           var groupedRoomConsumption = new GroupedRoomConsumption();
           groupedRoomConsumption.RoomType = roomPerRoomType.Key;
           long consumption = roomPerRoomType.Sum(room => room.Plugs
               .SelectMany(plug => plug.Consumptions)
               .Where(consumption => consumption.Time.Date == DateTime.Today.Date)
               .Sum(consumption => consumption.TotalEnergy));
           groupedRoomConsumption.Consumption = consumption;
           groupedPerDayRoomConsumption.GroupedRooms.Add(groupedRoomConsumption);
       }
        
       return groupedPerDayRoomConsumption;
    }
    
    
    public async Task<GetPerDayRoomConsumptionResponse> GetPerDayRoomConsumption()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub)); 
        var rooms = await _dbContext.Rooms.Include(x => x.RoomType).Include(x => x.Plugs).ThenInclude(p => p.Consumptions).Where(x => x.HouseholdId == user!.HouseholdId).ToListAsync();
        GetPerDayRoomConsumptionResponse perDayRoomConsumption = new GetPerDayRoomConsumptionResponse(){ Rooms = new List<RoomConsumption>()};
        
        foreach(var room in rooms)
        {
            var totalEnergyConsumption = room.Plugs
                .SelectMany(plug => plug.Consumptions)
                .Where(consumption => consumption.Time.Date == DateTime.Today.Date)
                .Sum(consumption => consumption.TotalEnergy);
            
            perDayRoomConsumption.Rooms.Add(new RoomConsumption()
            {
                RoomId = room.Id,
                Name = room.Name,
                RoomType = room.RoomType.Name,
                Consumption = totalEnergyConsumption
            });
        }
        
        return perDayRoomConsumption;
    }

    public async Task<GetPlugsPerRoomOverviewResponse> GetTotalRoomDetails()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub)); 
        var rooms = await _dbContext.Rooms.Include(y => y.RoomType).Include(x => x.Plugs).Where(x => x.HouseholdId == user!.HouseholdId).ToListAsync();
        var totalRoomDetails = new GetPlugsPerRoomOverviewResponse(){ Rooms = new List<RoomDetails>() };

        foreach (var room in rooms)
        {
            var plugsCount = room.Plugs.Count();
            var activePlugsCount = room.Plugs.Count(x => x.IsOn == true);
            totalRoomDetails.Rooms.Add(new RoomDetails()
            {
                RoomId = room.Id,
                Name = room.Name,
                RoomType = room.RoomType.Name,
                TotalPlugsCount = plugsCount,
                ActivePlugsCount = activePlugsCount
            });
        }
        
        return totalRoomDetails;
    }

    public async Task<GetAllSourcesResponse> GetAllSources(GetAllSourcesRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.Include(x => x.Household).ThenInclude(y => y.PowerSources).FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub));
        var powerSources = user.Household.PowerSources.ToList();
        if(powerSources.Count == 0) throw new ArgumentException("No PowerSources found for this user's household");
        
        
        return new GetAllSourcesResponse()
        {
            Sources = new List<SourceDto>(powerSources.Select(x => new SourceDto()
            {
                Id = x.Id,
                Name = x.Name,
                MaxCapacity = x.MaxCapacity,
                HouseholdId = x.HouseholdId
            }).ToList())
        };

    }
}