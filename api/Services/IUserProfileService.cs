using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task<UserProfile> GetOrCreateUserAsync(string userId, string displayName, string? email = null);
    Task<UserProfile> UpdateLastLoginAsync(string userId);
    Task<int> GetUserCountAsync();
    Task<bool> CanRegisterAsync();
}
