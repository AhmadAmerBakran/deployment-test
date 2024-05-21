using System.Data.Common;
using Core.Exceptions;
using Core.Interfaces;
using Core.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Repositories;

public class CarLogRepository : ICarLogRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<CarLogRepository> _logger;

    public CarLogRepository(NpgsqlDataSource dataSource, ILogger<CarLogRepository> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        const string query = "SELECT * FROM car_log.users";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Fetching all users.");
                return await connection.QueryAsync<User>(query);
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching users.");
            throw new AppException("A database error occurred while fetching users. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching users.");
            throw new AppException("A database error occurred while fetching users. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching users.");
            throw new AppException("An unexpected error occurred while fetching users. Please try again later.");
        }
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        const string query = @$"SELECT id as {nameof(User.Id)}, nickname as {nameof(User.NickName)} FROM car_log.users WHERE id = @Id";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Fetching user with ID: {UserId}", id);
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching the user.");
            throw new AppException("A database error occurred while fetching the user. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching the user.");
            throw new AppException("A database error occurred while fetching the user. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching the user.");
            throw new AppException("An unexpected error occurred while fetching the user. Please try again later.");
        }
    }

    public async Task<User> AddUserAsync(Guid userId, string nickname)
    {
        const string checkQuery = "SELECT COUNT(1) FROM car_log.users WHERE id = @UserId";
        const string insertQuery = @"
            INSERT INTO car_log.users (id, nickname) 
            VALUES (@UserId, @NickName) 
            RETURNING id, nickname";

        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Checking if user with ID: {UserId} exists.", userId);
                var userExists = await connection.ExecuteScalarAsync<bool>(checkQuery, new { UserId = userId });
                if (!userExists)
                {
                    _logger.LogInformation("User does not exist. Adding user with ID: {UserId}", userId);
                    return await connection.QueryFirstAsync<User>(insertQuery,
                        new { UserId = userId, NickName = nickname });
                }
                else
                {
                    _logger.LogInformation("User with ID: {UserId} already exists.", userId);
                    return await GetUserByIdAsync(userId);
                }
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while adding the user.");
            throw new AppException("A database error occurred while adding the user. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while adding the user.");
            throw new AppException("A database error occurred while adding the user. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while adding the user.");
            throw new AppException("An unexpected error occurred while adding the user. Please try again later.");
        }
    }

    public async Task<IEnumerable<CarNotification>> GetNotificationsByUserIdAsync(int userId)
    {
        const string query = @"
            SELECT user_id, from_topic, to_topic, message, message_at 
            FROM car_log.car_notifications 
            WHERE user_id = @UserId";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Fetching notifications for user with ID: {UserId}", userId);
                return await connection.QueryAsync<CarNotification>(query, new { UserId = userId });
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching notifications.");
            throw new AppException("A database error occurred while fetching notifications. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching notifications.");
            throw new AppException("A database error occurred while fetching notifications. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching notifications.");
            throw new AppException("An unexpected error occurred while fetching notifications. Please try again later.");
        }
    }

    public async Task<CarNotification> AddNotificationAsync(Guid userId, string fromTopic, string toTopic, string message)
    {
        const string query = @"
            INSERT INTO car_log.car_notifications (user_id, from_topic, to_topic, message, message_at) 
            VALUES (@UserId, @FromTopic, @ToTopic, @Message, @MessageAt) 
            RETURNING *;";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Adding notification for user with ID: {UserId}", userId);
                return await connection.QueryFirstAsync<CarNotification>(query, new
                {
                    UserId = userId,
                    FromTopic = fromTopic,
                    ToTopic = toTopic,
                    Message = message,
                    MessageAt = DateTime.Now
                });
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while adding the notification.");
            throw new AppException("A database error occurred while adding the notification. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while adding the notification.");
            throw new AppException("A database error occurred while adding the notification. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while adding the notification.");
            throw new AppException("An unexpected error occurred while adding the notification. Please try again later.");
        }
    }

    public async Task<IEnumerable<CarNotification>> GetCarLog()
    {
        const string query = @$"SELECT user_id as {nameof(CarNotification.UserId)}, from_topic as {nameof(CarNotification.FromTopic)},
             to_topic as {nameof(CarNotification.ToTopic)}, message as {nameof(CarNotification.Message)}, 
             message_at as {nameof(CarNotification.MessageAt)} FROM car_log.car_notifications ORDER BY message_at DESC LIMIT 10";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Fetching car log.");
                return await connection.QueryAsync<CarNotification>(query);
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching the car log.");
            throw new AppException("A database error occurred while fetching the car log. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching the car log.");
            throw new AppException("A database error occurred while fetching the car log. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching the car log.");
            throw new AppException("An unexpected error occurred while fetching the car log. Please try again later.");
        }
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        const string query = "DELETE FROM car_log.users WHERE id = @Id";
        try
        {
            using (var connection = _dataSource.CreateConnection())
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", id);
                return await connection.ExecuteAsync(query, new { Id = id });
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "A database error occurred while deleting the user.");
            throw new AppException("A database error occurred while deleting the user. Please try again later.");
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "A database error occurred while deleting the user.");
            throw new AppException("A database error occurred while deleting the user. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting the user.");
            throw new AppException("An unexpected error occurred while deleting the user. Please try again later.");
        }
    }
}