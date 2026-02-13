using Med_Vehicle.Models;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;

namespace Med_Vehicle.Services;


public class UserService
{
    // MongoDB collection for users
    private readonly IMongoCollection<User> _users;

    public UserService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDB"));
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
    if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;
    return user;
}

// Retrieve user by email only
public async Task<User?> GetUserByEmailAsync(string email) =>
    await _users.Find(u => u.Email == email).FirstOrDefaultAsync();





//method to update user info with optional password change
public async Task UpdateUserAsync(User updatedUser, string? newPassword = null)
{
    // If a new password is provided will hash it before updating
    if (!string.IsNullOrWhiteSpace(newPassword))
    {
        updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
    }

    //Replace the existing user document with the updated one
    await _users.ReplaceOneAsync(u => u.Id == updatedUser.Id, updatedUser);
}

}

