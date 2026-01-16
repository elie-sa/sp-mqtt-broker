using SPBackend.DTOs;

namespace SPBackend.Requests.Queries.GetPlugsPerRoom;

public class GetPlugsPerRoomResponse
{
    public List<PlugDetail> Plugs { get; set; }
}