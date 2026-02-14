using Med_Vehicle.Models;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Logging; // Added for logging

namespace Med_Vehicle.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Attempting to initialize UserService...");

        // 1. Get Connection String
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogCritical("FATAL: CONNECTION_STRING is missing or empty.");
            throw new InvalidOperationException("Missing 'CONNECTION_STRING' environment variable.");
        }
        else
        {
            // Log that we found it (BUT DO NOT log the full string for security)
            _logger.LogInformation($"Connection string found (Length: {connectionString.Length}). Connecting to MongoDB...");
        }

        try 
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("MedVehicleDB");
            _users = database.GetCollection<User>("Users");
            
            // Test the connection immediately by counting users (lightweight check)
            var count = _users.CountDocuments(FilterDefinition<User>.Empty);
            _logger.LogInformation($"Successfully connected to MongoDB. Found {count} users in database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB in UserService constructor.");
            throw;
        }
    }

    public async Task CreateUserAsync(User user)
    {
        _logger.LogInformation($"Creating new user: {user.Email}");
        user.Password = BC.HashPassword(user.Password);
        await _users.InsertOneAsync(user);
        _logger.LogInformation($"User {user.Email} created successfully.");
    }

    public async Task<User?> GetUserAsync(string email, string password)
    {
        _logger.LogInformation($"Login attempt for: {email}");

        try
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning($"Login failed: User '{email}' not found in database.");
                return null;
            }

            if (!BC.Verify(password, user.Password))
            {
                _logger.LogWarning($"Login failed: Incorrect password for '{email}'.");
                return null;
            }

            _logger.LogInformation($"Login successful for: {email}");
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during GetUserAsync for {email}");
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<User?> GetUserByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
}