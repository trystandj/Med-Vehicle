using Med_Vehicle.Models;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;

namespace Med_Vehicle.Services;

public class UserService
{
    // MongoDB collection for users
    private readonly IMongoCollection<User> _users;

    public UserService()
    {
        // 1. Get the connection string directly from Environment Variables
        // Make sure your .env or Railway variable is named "CONNECTION_STRING"
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        // 2. Safety Check: This prevents the app from crashing silently if the var is missing
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Missing 'CONNECTION_STRING' environment variable. Please check your .env file or Railway dashboard.");
        }

        var client = new MongoClient(connectionString);
        
        // Optional: You can also put "MedVehicleDB" in an env variable if you want
        var database = client.GetDatabase("MedVehicleDB");
        
        _users = database.GetCollection<User>("Users");
    }

    // Create a new user with hashed password
    public async Task CreateUserAsync(User user)
    {
        user.Password = BC.HashPassword(user.Password);
        await _users.InsertOneAsync(user);
    }

    // Retrieve user by email and verify password
    public async Task<User?> GetUserAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        // Fixed the verify call to use the alias 'BC' for consistency
        if (user == null || !BC.Verify(password, user.Password)) return null;
        return user;
    }

    // Retrieve user by email only
    public async Task<User?> GetUserByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    // Retrieve user by id
    public async Task<User?> GetUserByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
}