using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudyTracker.Api.Models;
using StudyTracker.Api.Services;
using System.Net;
using Newtonsoft.Json;

namespace StudyTracker.Api.Functions;

public class StudySessionFunctions
{
    private readonly ILogger<StudySessionFunctions> _logger;
    private readonly IStudySessionService _studySessionService;

    public StudySessionFunctions(ILogger<StudySessionFunctions> logger, IStudySessionService studySessionService)
    {
        _logger = logger;
        _studySessionService = studySessionService;
    }

    [Function("GetStudySessions")]
    public async Task<HttpResponseData> GetStudySessions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sessions")] HttpRequestData req)
    {
        _logger.LogInformation("Getting study sessions");

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var sessions = await _studySessionService.GetSessionsAsync(userId);
            return await CreateJsonResponse(req, sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study sessions");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("GetStudySession")]
    public async Task<HttpResponseData> GetStudySession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sessions/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Getting study session {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var session = await _studySessionService.GetSessionAsync(id, userId);
            if (session == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study session not found");
            }

            return await CreateJsonResponse(req, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study session {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("CreateStudySession")]
    public async Task<HttpResponseData> CreateStudySession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sessions")] HttpRequestData req)
    {
        _logger.LogInformation("Creating study session");

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var session = JsonConvert.DeserializeObject<StudySession>(requestBody);

            if (session == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid session data");
            }

            // Server-side validation
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(session.Category)) validationErrors.Add("Category is required");
            if (session.Hours <= 0) validationErrors.Add("Hours must be greater than zero");
            if (session.Date.Date > DateTime.UtcNow.Date) validationErrors.Add("Date cannot be in the future");
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, string.Join("; ", validationErrors));
            }

            session.UserId = userId;
            var createdSession = await _studySessionService.CreateSessionAsync(session);
            
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(createdSession);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating study session");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("UpdateStudySession")]
    public async Task<HttpResponseData> UpdateStudySession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "sessions/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Updating study session {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var session = JsonConvert.DeserializeObject<StudySession>(requestBody);

            if (session == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid session data");
            }

            // Server-side validation for updates
            var updateErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(session.Category)) updateErrors.Add("Category is required");
            if (session.Hours <= 0) updateErrors.Add("Hours must be greater than zero");
            if (session.Date.Date > DateTime.UtcNow.Date) updateErrors.Add("Date cannot be in the future");
            if (updateErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, string.Join("; ", updateErrors));
            }

            session.UserId = userId;
            var updatedSession = await _studySessionService.UpdateSessionAsync(id, session);
            
            if (updatedSession == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study session not found");
            }

            return await CreateJsonResponse(req, updatedSession);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating study session {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("DeleteStudySession")]
    public async Task<HttpResponseData> DeleteStudySession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "sessions/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Deleting study session {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var deleted = await _studySessionService.DeleteSessionAsync(id, userId);
            if (!deleted)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study session not found");
            }

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting study session {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static string? GetUserIdFromRequest(HttpRequestData req)
    {
        // Extract user ID from Azure Static Web Apps authentication headers
        req.Headers.TryGetValues("x-ms-client-principal-id", out var principalIds);
        return principalIds?.FirstOrDefault();
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, T data)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(data);
        return response;
    }

    private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }
}