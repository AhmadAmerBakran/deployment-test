using Core.Models;

namespace Core.Interfaces;

public interface ICarLogRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(Guid id);
    Task<User> AddUserAsync(Guid userId, string nickname);
    Task<IEnumerable<CarNotification>> GetNotificationsByUserIdAsync(int userId);
    Task<CarNotification> AddNotificationAsync(Guid userId, string fromTopic, string toTopic, string message);
    Task<int> DeleteUserAsync(int id);
    Task<IEnumerable<CarNotification>> GetCarLog();
}