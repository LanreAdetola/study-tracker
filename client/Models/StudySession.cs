namespace StudyTracker.Models
{
    public class StudySession
    {
        public required string Category { get; set; }
        public required double Hours { get; set; }
        public required string Notes { get; set; }
        public required DateTime Date { get; set; }
    }
}
