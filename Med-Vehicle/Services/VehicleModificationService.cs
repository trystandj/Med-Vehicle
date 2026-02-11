namespace Med_Vehicle.MongoDB;

using Med_Vehicle.Models;
using Med_Vehicle.Infrastructure;
using global::MongoDB.Driver;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class VehicleModificationService
{
    private readonly IMongoCollection<VehicleModifications> _modCollection;
    private readonly AuthenticationStateProvider _authStateProvider;

    public VehicleModificationService(NamedCollection dbContext, AuthenticationStateProvider authStateProvider)
    {
        // collection is named "VehicleModifications"
        _modCollection = dbContext.GetCollection<VehicleModifications>("VehicleModifications");
        _authStateProvider = authStateProvider;
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    // --- CRUD Operations ---

    // READ: Get All Modifications for a specific Car
    public async Task<List<VehicleModifications>> GetModificationsByCarIdAsync(string carId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return new List<VehicleModifications>();

        return await _modCollection
            .Find(x => x.CarId == carId && x.UserId == userId)
            .SortByDescending(x => x.DateInstalled)
            .ToListAsync();
    }

    // CREATE: Automatically assign the correct User ID
    public async Task CreateAsync(VehicleModifications newMod)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User not logged in.");

        newMod.UserId = userId;
        
        await _modCollection.InsertOneAsync(newMod);
    }
        
    // READ: Get Single Modification Item by PublicId
    public async Task<VehicleModifications?> GetByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();
        
        return await _modCollection
            .Find(x => x.PublicId == publicId && x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // UPDATE: Find by PublicId 
    public async Task UpdateByPublicIdAsync(string publicId, VehicleModifications updatedMod)
    {
        var userId = await GetCurrentUserIdAsync();

        updatedMod.UserId = userId; 

        var result = await _modCollection.ReplaceOneAsync(
            x => x.PublicId == publicId && x.UserId == userId, 
            updatedMod);

        if (result.MatchedCount == 0)
        {
            throw new UnauthorizedAccessException("Modification record not found or you do not own it.");
        }
    }

    // DELETE: Find by PublicId 
    public async Task RemoveByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();

        var result = await _modCollection.DeleteOneAsync(
            x => x.PublicId == publicId && x.UserId == userId);
    }
}