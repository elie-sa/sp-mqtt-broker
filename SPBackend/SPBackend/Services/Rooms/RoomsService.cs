using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Models;
using SPBackend.Requests.Queries.GetPlugsPerRoom;
using SPBackend.Requests.Queries.GetRooms;
using SPBackend.Requests.Queries.GetTodayRoomConsumptionSummary;
using SPBackend.Requests.Queries.GetTodayRoomCostSummary;
using SPBackend.Services.CurrentUser;

namespace SPBackend.Services.Rooms;

public class RoomsService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RoomsService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<GetRoomsResponse> GetAllRooms(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken); 
        var rooms = await _dbContext.Rooms.Include(y => y.RoomType).Include(x => x.Plugs).Where(x => x.HouseholdId == user!.HouseholdId).ToListAsync();
        var totalRoomDetails = new GetRoomsResponse(){ Rooms = new List<RoomDetails>() };

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

    public async Task<GetPlugsPerRoomResponse> GetPlugsPerRoom(long roomId, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.Include(x => x.Plugs).FirstOrDefaultAsync(x => x.Id == roomId);
        if(room is null) throw new KeyNotFoundException("Room not found.");

        var output = new GetPlugsPerRoomResponse(){ Plugs = new () };
        foreach (var plug in room.Plugs)
        {
            output.Plugs.Add(new PlugDetail()
            {
                Id = plug.Id,
                Name = plug.Name,
                IsConstant = plug.IsConstant,
                IsOn = plug.IsOn
            });
        }

        return output;
    }

    public async Task<GetTodayRoomConsumptionSummaryResponse> GetTodayRoomConsumptionSummary(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken);
        var rooms = await _dbContext.Rooms
            .Include(x => x.Plugs)
            .ThenInclude(p => p.Consumptions)
            .Where(x => x.HouseholdId == user!.HouseholdId)
            .ToListAsync(cancellationToken);

        var today = DateTime.Today.Date;
        var roomTotals = rooms.Select(room => new
            {
                room.Name,
                TotalWh = room.Plugs
                    .SelectMany(plug => plug.Consumptions)
                    .Where(consumption => consumption.Time.Date == today && consumption.Time.TimeOfDay == TimeSpan.Zero)
                    .Sum(consumption => consumption.TotalEnergy)
            })
            .ToList();

        var totalWh = roomTotals.Sum(x => x.TotalWh);
        var response = new GetTodayRoomConsumptionSummaryResponse();

        if (totalWh == 0)
        {
            response.Rooms = roomTotals
                .Select(x => new RoomConsumptionSummaryItem
                {
                    Name = x.Name,
                    Percentage = 0,
                    Kwh = 0
                })
                .OrderByDescending(x => x.Kwh)
                .ToList();

            return response;
        }

        response.Rooms = roomTotals
            .Select(x =>
            {
                var kwh = x.TotalWh / 1000;
                return new RoomConsumptionSummaryItem
                {
                    Name = x.Name,
                    Percentage = (x.TotalWh / totalWh) * 100,
                    Kwh = kwh
                };
            })
            .OrderByDescending(x => x.Kwh)
            .ToList();

        return response;
    }

    public async Task<GetTodayRoomCostSummaryResponse> GetTodayRoomCostSummary(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken);
        var rooms = await _dbContext.Rooms
            .Include(x => x.Plugs)
            .ThenInclude(p => p.Consumptions)
            .Where(x => x.HouseholdId == user!.HouseholdId)
            .ToListAsync(cancellationToken);

        var today = DateTime.Today.Date;
        var roomTotals = rooms.Select(room => new
            {
                room.Name,
                TotalCost = room.Plugs
                    .SelectMany(plug => plug.Consumptions)
                    .Where(consumption => consumption.Time.Date == today && consumption.Time.TimeOfDay == TimeSpan.Zero)
                    .Sum(consumption => consumption.TotalPrice)
            })
            .ToList();

        var totalCost = roomTotals.Sum(x => x.TotalCost);
        var response = new GetTodayRoomCostSummaryResponse();

        if (totalCost == 0)
        {
            response.Rooms = roomTotals
                .Select(x => new RoomCostSummaryItem
                {
                    Name = x.Name,
                    Percentage = 0,
                    Cost = 0
                })
                .OrderByDescending(x => x.Cost)
                .ToList();

            return response;
        }

        response.Rooms = roomTotals
            .Select(x => new RoomCostSummaryItem
            {
                Name = x.Name,
                Percentage = (x.TotalCost / totalCost) * 100,
                Cost = x.TotalCost
            })
            .OrderByDescending(x => x.Cost)
            .ToList();

        return response;
    }
}
