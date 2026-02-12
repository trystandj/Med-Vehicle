namespace Med_Vehicle.MongoDB;

using Med_Vehicle.Models;
using Med_Vehicle.Infrastructure; 
using global::MongoDB.Driver;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims; 

public class CarService
{
    private readonly IMongoCollection<Car> _carsCollection;
    private readonly IWebHostEnvironment _environment;
    private readonly AuthenticationStateProvider _authStateProvider;

    public CarService(NamedCollection dbContext, IWebHostEnvironment environment, AuthenticationStateProvider authStateProvider)
    {
        _carsCollection = dbContext.GetCollection<Car>("Cars");
        _environment = environment;
        _authStateProvider = authStateProvider;
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }


    // --- CRUD Operations ---

    // READ: Get All Cars (Filtered by logged-in user)
    public async Task<List<Car>> GetCarsAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return new List<Car>();

        return await _carsCollection.Find(x => x.UserId == userId).ToListAsync();
    }

    // CREATE: Automatically assign the correct User ID
    public async Task CreateAsync(Car newCar)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User not logged in.");

        newCar.UserId = userId; 
        await _carsCollection.InsertOneAsync(newCar);
    }
        
    // READ: Get by PublicId 
    public async Task<Car?> GetByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();
        

        return await _carsCollection
            .Find(x => x.PublicId == publicId && x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // UPDATE: Find by PublicId 
    public async Task UpdateByPublicIdAsync(string publicId, Car updatedCar)
    {
        var userId = await GetCurrentUserIdAsync();
        
        updatedCar.UserId = userId; 

        var result = await _carsCollection.ReplaceOneAsync(
            x => x.PublicId == publicId && x.UserId == userId, 
            updatedCar);

        if (result.MatchedCount == 0)
        {
            throw new UnauthorizedAccessException("Car not found or you do not own it.");
        }
    }

    // DELETE: Find by PublicId 
    public async Task RemoveByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();

        var car = await _carsCollection
            .Find(x => x.PublicId == publicId && x.UserId == userId)
            .FirstOrDefaultAsync();

        if (car is null) return; 

        if (!string.IsNullOrEmpty(car.ImageFileName))
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", car.ImageFileName);
            try 
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete file: {ex.Message}");
            }
        }

        await _carsCollection.DeleteOneAsync(x => x.PublicId == publicId && x.UserId == userId);
    }
}