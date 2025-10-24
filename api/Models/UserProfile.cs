using System;
using Newtonsoft.Json;

namespace StudyTracker.Api.Models;

public class UserProfile
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonProperty("email")]
    public string? Email { get; set; }
    
    [JsonProperty("joinedDate")]
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("lastLoginDate")]
    public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;
}
