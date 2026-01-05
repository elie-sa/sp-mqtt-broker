using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPBackend.Queries.GetPlugsPerRoom;
using SPBackend.Queries.GetRooms;

namespace SPBackend.Controllers;

[ApiController]
[Route("rooms")]
public class RoomController: ControllerBase
{
    private readonly IMediator _mediator;

    public RoomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> GetRooms()
    {
        return Ok(await _mediator.Send(new GetRoomsRequest()));
    }

    [Authorize]
    [HttpGet("{roomId}/plugs")]
    public async Task<IActionResult> GetPlugsPerRoom(long roomId)
    {
        return Ok(await _mediator.Send(new GetPlugsPerRoomRequest(){ RoomId = roomId }));
    }
    
}