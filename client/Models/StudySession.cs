namespace client.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    public class StudySession : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Hours must be greater than zero")]
        public double Hours { get; set; }

        public string? Notes { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date.Date > DateTime.Today)
            {
                yield return new ValidationResult("Date cannot be in the future", new[] { nameof(Date) });
            }
        }
    }
}
