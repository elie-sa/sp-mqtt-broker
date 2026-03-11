namespace SPBackend.Models;

public class NotificationToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string Token { get; set; }
}