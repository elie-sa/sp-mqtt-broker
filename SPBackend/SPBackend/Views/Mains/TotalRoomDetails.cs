namespace SPBackend.Views.Mains;

public class TotalRoomDetails
{
    public List<RoomDetails> Rooms { get; set; }
}

public class RoomDetails
{
    public long RoomId { get; set; }
    public string Name { get; set; }
    public string RoomType { get; set; }
    public long TotalPlugsCount { get; set; }
    public long ActivePlugsCount { get; set; }
}