using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace StudyTracker.Api.Models;

public class StudyGoal
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    [Required(ErrorMessage = "Goal name is required")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("type")]
    [Required(ErrorMessage = "Type is required")]
    public string Type { get; set; } = string.Empty; // "Subject" or "Certification"
    
    [JsonProperty("targetHours")]
    [Range(1, 10000, ErrorMessage = "Target hours must be between 1 and 10,000")]
    public double TargetHours { get; set; }
    
    [JsonProperty("currentHours")]
    public double CurrentHours { get; set; } = 0;
    
    [JsonProperty("targetDate")]
    public DateTime? TargetDate { get; set; }
    
    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
