namespace client.Models;

public class StudySessionStats
{
    public int TotalSessions { get; set; }
    public double TotalHours { get; set; }
    public double AverageHoursPerDay { get; set; }
    public Dictionary<string, double> HoursByCategory { get; set; } = new();
    public List<DailyHours> DailyBreakdown { get; set; } = new();
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double ThisWeekHours { get; set; }
    public double LastWeekHours { get; set; }
}

public class DailyHours
{
    public DateTime Date { get; set; }
    public double Hours { get; set; }
}

public class GoalProgressPoint
{
    public string Date { get; set; } = string.Empty;
    public double CumulativeHours { get; set; }
}

public class GoalProgressDataset
{
    public string GoalName { get; set; } = string.Empty;
    public double TargetHours { get; set; }
    public List<GoalProgressPoint> Points { get; set; } = new();
}
