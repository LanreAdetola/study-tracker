namespace StudyTracker.Api.Models;

public class StudySessionStats
{
    public int TotalSessions { get; set; }
    public double TotalHours { get; set; }
    public double AverageHoursPerDay { get; set; }
    public Dictionary<string, double> HoursByCategory { get; set; } = new();
    public List<DailyHours> DailyBreakdown { get; set; } = new();
}

public class DailyHours
{
    public DateTime Date { get; set; }
    public double Hours { get; set; }
}
