namespace Core.Models;

public class CarNotification
{
    public Guid UserId { get; set; }
    public string FromTopic { get; set; }
    public string ToTopic { get; set; }
    public string Message { get; set; }
    public DateTime MessageAt { get; set; }
}