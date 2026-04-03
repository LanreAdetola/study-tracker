using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace StudyTracker.Api.Models;

public class StudySession
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("category")]
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = string.Empty;
    
    [JsonProperty("hours")]
    [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue, ErrorMessage = "Hours must be greater than zero")]
    public double Hours { get; set; }
    
    [JsonProperty("notes")]
    public string Notes { get; set; } = string.Empty;
    
    [JsonProperty("date")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTime Date { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}