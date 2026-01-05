using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.DTOs;
using SPBackend.Models;
using SPBackend.Queries.GetPlugsPerRoom;
using SPBackend.Queries.GetRooms;
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub)); 
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
    
    
}