using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Views.Mains;

namespace SPBackend.Services.Mains;

public class PowerSourceService
{
    private readonly IAppDbContext _context;

    public PowerSourceService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PowerSourceViewModel> GetPowerSource(int householdId)
    {
        var mainsLog = await _context.MainsLogs.Include(x => x.PowerSource).OrderByDescending(p => p.Time).FirstOrDefaultAsync(x => x.Household.Id == householdId);
        if (mainsLog == null)
        {
            throw new KeyNotFoundException("The household id provided is invalid or no logs have been recorded yet.");
        }
        
        return new PowerSourceViewModel()
        {
            PowerSourceId = mainsLog.PowerSource.Id,
            Name = mainsLog.PowerSource.Name,
            Voltage = mainsLog.Voltage,
            LastUpdated = mainsLog.Time,
        };
    }

    public async Task<GroupedPerDayRoomConsumptionViewModel> GetGroupedPerDayRoomConsumption(int householdId)
    {
       var groupedPerDayRoomConsumption = new GroupedPerDayRoomConsumptionViewModel(){ GroupedRooms = new List<GroupedRoomConsumption>() };
       var roomsPerRoomTypes = await _context.Rooms.Include(x => x.RoomType).Include(x => x.Plugs).ThenInclude(p => p.Consumptions)
           .Where(x => x.HouseholdId == householdId).GroupBy(x => x.RoomType.Name).ToListAsync();

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
    
    
    public async Task<PerDayRoomConsumptionViewModel> GetPerDayRoomConsumption(int householdId)
    {
        var rooms = await _context.Rooms.Include(x => x.RoomType).Include(x => x.Plugs).ThenInclude(p => p.Consumptions).Where(x => x.HouseholdId == householdId).ToListAsync();
        PerDayRoomConsumptionViewModel perDayRoomConsumption = new PerDayRoomConsumptionViewModel(){ Rooms = new List<RoomConsumption>()};
        
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

    public async Task<TotalRoomDetails> GetTotalRoomDetails(int householdId)
    {
        var rooms = await _context.Rooms.Include(y => y.RoomType).Include(x => x.Plugs).Where(x => x.HouseholdId == householdId).ToListAsync();
        var totalRoomDetails = new TotalRoomDetails(){ Rooms = new List<RoomDetails>() };

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
    
}