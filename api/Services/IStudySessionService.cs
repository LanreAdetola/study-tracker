using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StudyTracker.Api.Services;

public interface IStudySessionService
{
    Task<IEnumerable<StudySession>> GetSessionsAsync(string userId);
    Task<StudySession?> GetSessionAsync(string id, string userId);
    Task<StudySession> CreateSessionAsync(StudySession session);
    Task<StudySession?> UpdateSessionAsync(string id, StudySession session);
    Task<bool> DeleteSessionAsync(string id, string userId);
    Task<StudySessionStats> GetStatsAsync(string userId, DateTime? from, DateTime? to);
}