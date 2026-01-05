using SPBackend.DTOs;

namespace SPBackend.Queries.GetPlugsPerRoom;

public class GetPlugsPerRoomResponse
{
    public List<PlugDetail> Plugs { get; set; }
}