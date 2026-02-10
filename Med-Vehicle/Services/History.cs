namespace Med_Vehicle.MongoDB;

using Med_Vehicle.Models;
using Med_Vehicle.Infrastructure; 
using global::MongoDB.Driver;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims; 

public class HistoryService
{
    private readonly IMongoCollection<History> _historyCollection;
    private readonly AuthenticationStateProvider _authStateProvider;

    public HistoryService(NamedCollection dbContext, AuthenticationStateProvider authStateProvider)
    {
        _historyCollection = dbContext.GetCollection<History>("History");
        _authStateProvider = authStateProvider;
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    // --- CRUD Operations ---

    public async Task<List<History>> GetHistoryByCarIdAsync(string carId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return new List<History>();

        return await _historyCollection
            .Find(x => x.CarId == carId && x.UserId == userId)
            .SortByDescending(x => x.DateOfEvent) 
            .ToListAsync();
    }

    // CREATE: Automatically assign the correct User ID
    public async Task CreateAsync(History newHistory)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User not logged in.");

        newHistory.UserId = userId; 
        
        await _historyCollection.InsertOneAsync(newHistory);
    }
        
    // READ: Get Single History Item by PublicId
    public async Task<History?> GetByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();
        
        return await _historyCollection
            .Find(x => x.PublicId == publicId && x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // UPDATE: Find by PublicId (AND verify ownership)
    public async Task UpdateByPublicIdAsync(string publicId, History updatedHistory)
    {
        var userId = await GetCurrentUserIdAsync();

        updatedHistory.UserId = userId; 


        var result = await _historyCollection.ReplaceOneAsync(
            x => x.PublicId == publicId && x.UserId == userId, 
            updatedHistory);

        if (result.MatchedCount == 0)
        {
            throw new UnauthorizedAccessException("History record not found or you do not own it.");
        }
    }

    // DELETE: Find by PublicId (AND verify ownership)
    public async Task RemoveByPublicIdAsync(string publicId)
    {
        var userId = await GetCurrentUserIdAsync();

        var result = await _historyCollection.DeleteOneAsync(
            x => x.PublicId == publicId && x.UserId == userId);

    }
}