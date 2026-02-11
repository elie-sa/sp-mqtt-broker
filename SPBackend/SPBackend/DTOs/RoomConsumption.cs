namespace SPBackend.DTOs;

public class RoomConsumption
{
    public long RoomId { get; set; }
    public string Name { get; set; }
    public string RoomType { get; set; }
    public double Consumption { get; set; }
}