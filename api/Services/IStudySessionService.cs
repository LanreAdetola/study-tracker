using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public interface IStudySessionService
{
    Task<IEnumerable<StudySession>> GetSessionsAsync(string userId);
    Task<StudySession?> GetSessionAsync(string id, string userId);
    Task<StudySession> CreateSessionAsync(StudySession session);
    Task<StudySession?> UpdateSessionAsync(string id, StudySession session);
    Task<bool> DeleteSessionAsync(string id, string userId);
}