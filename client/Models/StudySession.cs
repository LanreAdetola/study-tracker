namespace client.Models
{
    public class StudySession
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Hours { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
