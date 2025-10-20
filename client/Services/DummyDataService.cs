using StudyTracker.Models;

namespace StudyTracker.Services
{
    public class DummyDataService
    {
        public List<StudySession> GetSessions()
        {
            return new List<StudySession>
            {
                new() { Category = "Azure Fundamentals", Hours = 2.5, Notes = "Reviewed core services", Date = DateTime.Today },
                new() { Category = "C# Practice", Hours = 1.5, Notes = "Worked on recursion exercises", Date = DateTime.Today.AddDays(-1) }
            };
        }
    }
}
