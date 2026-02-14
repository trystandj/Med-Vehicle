namespace Med_Vehicle.MongoDB;

using Med_Vehicle.Models;
using Med_Vehicle.Infrastructure;
using global::MongoDB.Driver;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

public class CarService
{
    private readonly IMongoCollection<Car> _carsCollection;
    private readonly IWebHostEnvironment _environment;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<CarService> _logger;

    // Conostructor along with initialization of MongoDB collection, environment, authentication state provider, and logger
    public CarService(
        NamedCollection dbContext, 
        IWebHostEnvironment environment, 
        AuthenticationStateProvider authStateProvider,
        ILogger<CarService> logger)
    {
        _carsCollection = dbContext.GetCollection<Car>("Cars");
        _environment = environment;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    // Helper method to get the current user's ID from the authentication state
    private async Task<string> GetCurrentUserIdAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user authentication state.");
            return string.Empty;
        }
    }

    // CRUD operations for Car records, all of which include error handling and logging
    public async Task<List<Car>> GetCarsAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return new List<Car>();

            return await _carsCollection.Find(x => x.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cars from MongoDB.");
            return new List<Car>(); 
        }
    }

    // Create a new car record, associating it with the current user and handling any exceptions
    public async Task CreateAsync(Car newCar)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User not logged in.");

            newCar.UserId = userId;
            await _carsCollection.InsertOneAsync(newCar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create car record.");
            throw; 
        }
    }

    // Retrieve a specific car by its public ID, ensuring it belongs to the current user and handling any errors that may occur
    public async Task<Car?> GetByPublicIdAsync(string publicId)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return null;

            return await _carsCollection
                .Find(x => x.PublicId == publicId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching car {PublicId}", publicId);
            return null;
        }
    }

    // Update an existing car record by its public ID, ensuring it belongs to the current user and handling any exceptions that may arise during the update process
    public async Task UpdateByPublicIdAsync(string publicId, Car updatedCar)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

            updatedCar.UserId = userId;
            var result = await _carsCollection.ReplaceOneAsync(
                x => x.PublicId == publicId && x.UserId == userId,
                updatedCar);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Update failed: Car {PublicId} not found for user {UserId}", publicId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating car {PublicId}", publicId);
            throw;
        }
    }

    // Remove a car record by its public ID, ensuring it belongs to the current user, and also handle the deletion of any associated image file while logging any errors that may occur during the process
    public async Task RemoveByPublicIdAsync(string publicId)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var car = await _carsCollection
                .Find(x => x.PublicId == publicId && x.UserId == userId)
                .FirstOrDefaultAsync();

            if (car is null) return;

            var rootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            
            if (!string.IsNullOrEmpty(car.ImageFileName))
            {
                var filePath = Path.Combine(rootPath, "uploads", car.ImageFileName);
                
                try
                {
                    if (File.Exists(filePath)) 
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File deletion failed at {Path}", filePath);
                }
            }

            await _carsCollection.DeleteOneAsync(x => x.PublicId == publicId && x.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing car {PublicId}", publicId);
            throw;
        }
    }
}