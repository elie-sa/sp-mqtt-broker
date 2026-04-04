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
using SPBackend.Requests.Queries.GetMonthlyPowerSourceBill;
using SPBackend.Requests.Queries.GetPerDayRoomConsumption;
using SPBackend.Requests.Queries.GetPlugsPerRoomOverview;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceCosts;
using SPBackend.Requests.Queries.GetWeeklyPowerSourceSessionHours;
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

        var powerSources = await _dbContext.PowerSources
            .Where(x => x.HouseholdId == user.HouseholdId)
            .ToListAsync(cancellationToken);

        var weeklyConsumptions = await _dbContext.MainsConsumptions
            .Include(x => x.PowerSource)
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfWeek
                        && x.Time <= endOfWeek)
            .ToListAsync(cancellationToken);

        var totalsBySource = weeklyConsumptions
            .GroupBy(x => x.PowerSourceId)
            .Select(group => new
            {
                PowerSourceId = group.Key,
                Total = group.Sum(x => x.Consumption)
            })
            .ToDictionary(x => x.PowerSourceId, x => x.Total);

        var totalConsumption = totalsBySource.Values.Sum();
        var response = new GetWeeklyPowerSourceUsageResponse();

        if (totalConsumption == 0)
        {
            response.PowerSources = powerSources
                .Select(x => new PowerSourceUsagePercentage
                {
                    Name = x.Name,
                    Percentage = 0
                })
                .OrderByDescending(x => x.Percentage)
                .ToList();

            return response;
        }

        response.PowerSources = powerSources
            .Select(source =>
            {
                totalsBySource.TryGetValue(source.Id, out var total);
                return new PowerSourceUsagePercentage
                {
                    Name = source.Name,
                    Percentage = totalConsumption == 0 ? 0 : (total / totalConsumption) * 100
                };
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return response;
    }

    public async Task<GetMonthlyPowerSourceBillResponse> GetMonthlyPowerSourceBill(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var startOfThisMonth = new DateOnly(today.Year, today.Month, 1);

        var powerSources = await _dbContext.PowerSources
            .Where(x => x.HouseholdId == user.HouseholdId)
            .ToListAsync(cancellationToken);

        var monthlyConsumptions = await _dbContext.MainsConsumptions
            .Include(x => x.PowerSource)
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfThisMonth
                        && x.Time <= today)
            .ToListAsync(cancellationToken);

        var totalsBySource = monthlyConsumptions
            .GroupBy(x => x.PowerSourceId)
            .Select(group => new
            {
                PowerSourceId = group.Key,
                TotalWh = group.Sum(x => x.Consumption)
            })
            .ToDictionary(x => x.PowerSourceId, x => x.TotalWh);

        var response = new GetMonthlyPowerSourceBillResponse();

        response.PowerSources = powerSources
            .Select(source =>
            {
                totalsBySource.TryGetValue(source.Id, out var totalWh);
                var kwh = totalWh / 1000;
                var cost = kwh * source.CostPerKwh;
                return new MonthlyPowerSourceBillItem
                {
                    Name = source.Name,
                    Kwh = kwh,
                    Cost = cost
                };
            })
            .OrderByDescending(x => x.Cost)
            .ToList();

        response.TotalCost = response.PowerSources.Sum(x => x.Cost);

        return response;
    }

    public async Task<GetWeeklyPowerSourceCostsResponse> GetWeeklyPowerSourceCosts(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var diff = ((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var startOfWeek = DateOnly.FromDateTime(DateTime.Today.AddDays(-diff));
        var endOfWeek = startOfWeek.AddDays(6);

        var powerSources = await _dbContext.PowerSources
            .Where(x => x.HouseholdId == user.HouseholdId)
            .ToListAsync(cancellationToken);

        var weeklyConsumptions = await _dbContext.MainsConsumptions
            .Include(x => x.PowerSource)
            .Where(x => x.PowerSource.HouseholdId == user.HouseholdId
                        && x.Time >= startOfWeek
                        && x.Time <= endOfWeek)
            .ToListAsync(cancellationToken);

        var totalsByDayAndSource = weeklyConsumptions
            .GroupBy(x => new { x.Time, x.PowerSourceId })
            .Select(group => new
            {
                group.Key.Time,
                group.Key.PowerSourceId,
                TotalWh = group.Sum(x => x.Consumption)
            })
            .ToList();

        var totalsLookup = totalsByDayAndSource
            .ToDictionary(x => (x.Time, x.PowerSourceId), x => x.TotalWh);

        var response = new GetWeeklyPowerSourceCostsResponse();

        for (var date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
        {
            var dayTotals = powerSources
                .Select(source =>
                {
                    totalsLookup.TryGetValue((date, source.Id), out var totalWh);
                    var kwh = totalWh / 1000;
                    var cost = kwh * source.CostPerKwh;
                    return new
                    {
                        source.Name,
                        Cost = cost
                    };
                })
                .ToList();

            var totalCost = dayTotals.Sum(x => x.Cost);

            var day = new DailyPowerSourceCosts
            {
                Day = date.DayOfWeek.ToString(),
                PowerSources = dayTotals
                    .Select(x => new PowerSourceDailyCost
                    {
                        Name = x.Name,
                        Cost = x.Cost
                    })
                    .OrderByDescending(x => x.Cost)
                    .ToList()
            };

            response.Days.Add(day);
        }

        return response;
    }

    public async Task<GetWeeklyPowerSourceSessionHoursResponse> GetWeeklyPowerSourceSessionHours(long powerSourceId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var powerSource = await _dbContext.PowerSources
            .FirstOrDefaultAsync(x => x.Id == powerSourceId && x.HouseholdId == user.HouseholdId, cancellationToken);
        if (powerSource == null) throw new KeyNotFoundException("Power source not found");

        var nowUtc = DateTime.UtcNow;
        var diff = ((int)nowUtc.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var startOfWeek = nowUtc.Date.AddDays(-diff);
        var endExclusive = startOfWeek.AddDays(7);
        var cappedNowUtc = nowUtc > endExclusive ? endExclusive : nowUtc;

        var sessions = await _dbContext.PowerSourceSessions
            .Where(x => x.HouseholdId == user.HouseholdId
                        && x.PowerSourceId == powerSourceId
                        && x.StartTime < endExclusive
                        && (x.EndTime == null || x.EndTime > startOfWeek))
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        var days = new List<DailyPowerSourceHours>(7);

        for (var i = 0; i < 7; i++)
        {
            var dayStart = startOfWeek.AddDays(i);
            var dayEnd = dayStart.AddDays(1);
            var hours = 0.0;

            foreach (var session in sessions)
            {
                var sessionStart = session.StartTime;
                var sessionEnd = session.EndTime ?? cappedNowUtc;

                if (sessionEnd <= dayStart || sessionStart >= dayEnd)
                {
                    continue;
                }

                var overlapStart = sessionStart > dayStart ? sessionStart : dayStart;
                var overlapEnd = sessionEnd < dayEnd ? sessionEnd : dayEnd;

                if (overlapEnd > overlapStart)
                {
                    hours += (overlapEnd - overlapStart).TotalHours;
                }
            }

            days.Add(new DailyPowerSourceHours
            {
                Day = dayStart.DayOfWeek.ToString(),
                Hours = Math.Round(hours, 1, MidpointRounding.AwayFromZero)
            });
        }

        return new GetWeeklyPowerSourceSessionHoursResponse
        {
            PowerSourceName = powerSource.Name,
            Days = days
        };
    }

}
