using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Requests.Commands.UpdatePowerSourceCost;
using SPBackend.Requests.Queries.GetAllSources;
using SPBackend.Requests.Queries.GetGroupedPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;
using SPBackend.Requests.Queries.GetPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPlugsPerRoomOverview;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceUsage;
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
            throw new KeyNotFoundException("The household is invalid or no logs have been recorded yet.");
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
           var consumption = roomPerRoomType.Sum(room => room.Plugs
               .SelectMany(plug => plug.Consumptions)
               .Where(consumption => consumption.Time.Date == DateTime.Today.Date && consumption.Time.TimeOfDay == TimeSpan.Zero)
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
                .Where(consumption => consumption.Time.Date == DateTime.Today.Date && consumption.Time.TimeOfDay == TimeSpan.Zero)
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

    public async Task<UpdatePowerSourceCostResponse> UpdatePowerSourceCost(UpdatePowerSourceCostRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var powerSource = await _dbContext.PowerSources.FirstOrDefaultAsync(
            x => x.Id == request.PowerSourceId && x.HouseholdId == user.HouseholdId,
            cancellationToken);
        if (powerSource == null) throw new KeyNotFoundException("No power source was found");

        powerSource.CostPerKwh = request.CostPerKwh;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdatePowerSourceCostResponse()
        {
            Message = $"Power source {powerSource.Id} cost per kwh updated."
        };
    }

    public async Task<GetMonthlyConsumptionSummaryResponse> GetMonthlyConsumptionSummary(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var startOfThisMonth = new DateOnly(today.Year, today.Month, 1);

        var currentMonthConsumptions = await _dbContext.MainsConsumptions
            .Include(x => x.PowerSource)
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfThisMonth
                        && x.Time <= today)
            .ToListAsync(cancellationToken);

        var totalConsumptionThisMonth = currentMonthConsumptions.Sum(x => x.Consumption)/1000;
        var totalCostThisMonth = currentMonthConsumptions.Sum(x => x.Consumption * x.PowerSource.CostPerKwh)/1000;

        var lastMonth = today.AddMonths(-1);
        var startOfLastMonth = new DateOnly(lastMonth.Year, lastMonth.Month, 1);
        var lastMonthDay = Math.Min(today.Day, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        var endOfLastMonthRange = new DateOnly(lastMonth.Year, lastMonth.Month, lastMonthDay);

        var lastMonthConsumptions = await _dbContext.MainsConsumptions
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfLastMonth
                        && x.Time <= endOfLastMonthRange)
            .ToListAsync(cancellationToken);

        var totalConsumptionLastMonth = lastMonthConsumptions.Sum(x => x.Consumption)/1000;
        var totalCostLastMonth = lastMonthConsumptions.Sum(x => x.Consumption * x.PowerSource.CostPerKwh)/1000;
        double? differenceFromLastMonth = totalConsumptionLastMonth == 0
            ? null
            : ((totalConsumptionThisMonth - totalConsumptionLastMonth) / totalConsumptionLastMonth) * 100;
        double? costDifferenceFromLastMonth = totalCostLastMonth == 0
            ? null
            : ((totalCostThisMonth - totalCostLastMonth) / totalCostLastMonth) * 100;

        return new GetMonthlyConsumptionSummaryResponse()
        {
            TotalConsumptionThisMonth = totalConsumptionThisMonth,
            DifferenceFromLastMonth = differenceFromLastMonth,
            TotalCostThisMonth = totalCostThisMonth,
            CostDifferenceFromLastMonth = costDifferenceFromLastMonth
        };
    }

    public async Task<GetWeeklyPowerSourceUsageResponse> GetWeeklyPowerSourceUsage(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var today = DateTime.Today;
        var diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var startOfWeek = DateOnly.FromDateTime(today.AddDays(-diff));
        var endOfWeek = DateOnly.FromDateTime(today);

        var weeklyConsumptions = await _dbContext.MainsConsumptions
            .Include(x => x.PowerSource)
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfWeek
                        && x.Time <= endOfWeek)
            .ToListAsync(cancellationToken);

        var totalsBySource = weeklyConsumptions
            .GroupBy(x => x.PowerSource)
            .Select(group => new
            {
                Name = group.Key.Name,
                Total = group.Sum(x => x.Consumption)
            })
            .ToList();

        var totalConsumption = totalsBySource.Sum(x => x.Total);
        var response = new GetWeeklyPowerSourceUsageResponse();

        if (totalConsumption == 0)
        {
            return response;
        }

        response.PowerSources = totalsBySource
            .Select(x => new PowerSourceUsagePercentage
            {
                Name = x.Name,
                Percentage = (x.Total / totalConsumption) * 100
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return response;
    }

}
