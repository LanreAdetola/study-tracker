using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public interface IStudyGoalService
{
    Task<List<StudyGoal>> GetGoalsAsync(string userId);
    Task<StudyGoal?> GetGoalAsync(string id, string userId);
    Task<StudyGoal> CreateGoalAsync(StudyGoal goal);
    Task<StudyGoal?> UpdateGoalAsync(string id, StudyGoal goal);
    Task<bool> DeleteGoalAsync(string id, string userId);
    Task<int> GetGoalCountAsync(string userId);
}
