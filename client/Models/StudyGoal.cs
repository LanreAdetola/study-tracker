using System.ComponentModel.DataAnnotations;

namespace client.Models
{
    public class StudyGoal
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Goal name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty; // "Subject" or "Certification"

        [Range(1, 10000, ErrorMessage = "Target hours must be between 1 and 10,000")]
        public double TargetHours { get; set; }

        public double CurrentHours { get; set; } = 0; // Calculated from sessions

        public DateTime? TargetDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
