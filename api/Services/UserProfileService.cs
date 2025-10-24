using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public class UserProfileService : IUserProfileService
{
    private readonly Container _container;
    private const int MAX_USERS = 50;

    public UserProfileService(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("study-tracker");
        _container = database.GetContainer("users");
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<UserProfile>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<UserProfile> GetOrCreateUserAsync(string userId, string displayName, string? email = null)
    {
        // Check if user already exists
        var existingUser = await GetUserProfileAsync(userId);
        if (existingUser != null)
        {
            // Update last login
            return await UpdateLastLoginAsync(userId);
        }

        // Check if we can register a new user
        var canRegister = await CanRegisterAsync();
        if (!canRegister)
        {
            throw new InvalidOperationException("User registration is currently at capacity. Maximum of 50 users allowed.");
        }

        // Create new user profile
        var userProfile = new UserProfile
        {
            Id = userId,
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            JoinedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow,
            IsActive = true
        };

        await _container.CreateItemAsync(userProfile, new PartitionKey(userId));
        return userProfile;
    }

    public async Task<UserProfile> UpdateLastLoginAsync(string userId)
    {
        var userProfile = await GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            throw new InvalidOperationException($"User profile not found for userId: {userId}");
        }

        userProfile.LastLoginDate = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(userProfile, userProfile.Id, new PartitionKey(userId));
        return response.Resource;
    }

    public async Task<int> GetUserCountAsync()
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.isActive = true");
        var iterator = _container.GetItemQueryIterator<int>(query);
        
        var count = 0;
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            count = response.FirstOrDefault();
        }
        
        return count;
    }

    public async Task<bool> CanRegisterAsync()
    {
        var currentCount = await GetUserCountAsync();
        return currentCount < MAX_USERS;
    }
}
