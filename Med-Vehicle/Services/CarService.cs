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