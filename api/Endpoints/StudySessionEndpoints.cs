using StudyTracker.Api.Auth;
using StudyTracker.Api.Models;
using StudyTracker.Api.Services;
using Newtonsoft.Json;

namespace StudyTracker.Api.Endpoints;

public static class StudySessionEndpoints
{
    public static void MapStudySessionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/sessions", GetStudySessions);
        app.MapGet("/api/sessions/stats", GetStudySessionStats);
        app.MapGet("/api/sessions/{id:guid}", GetStudySession);
        app.MapPost("/api/sessions", CreateStudySession);
        app.MapPut("/api/sessions/{id:guid}", UpdateStudySession);
        app.MapDelete("/api/sessions/{id:guid}", DeleteStudySession);
    }

    private static async Task<IResult> GetStudySessions(HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger)
    {
        logger.LogInformation("Getting study sessions");

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var sessions = await studySessionService.GetSessionsAsync(userId);
            return Results.Ok(sessions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting study sessions");
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetStudySession(string id, HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger)
    {
        logger.LogInformation("Getting study session {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var session = await studySessionService.GetSessionAsync(id, userId);
            if (session == null)
            {
                return Results.Json(new { error = "Study session not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting study session {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CreateStudySession(HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger)
    {
        logger.LogInformation("Creating study session");

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var session = JsonConvert.DeserializeObject<StudySession>(requestBody);

            if (session == null)
            {
                return Results.Json(new { error = "Invalid session data" }, statusCode: StatusCodes.Status400BadRequest);
            }

            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(session.Category)) validationErrors.Add("Category is required");
            if (session.Hours <= 0) validationErrors.Add("Hours must be greater than zero");
            if (session.Date.Date > DateTime.UtcNow.Date) validationErrors.Add("Date cannot be in the future");
            if (validationErrors.Any())
            {
                return Results.Json(new { error = string.Join("; ", validationErrors) }, statusCode: StatusCodes.Status400BadRequest);
            }

            session.UserId = userId;
            var createdSession = await studySessionService.CreateSessionAsync(session);

            return Results.Json(createdSession, statusCode: StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating study session");
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateStudySession(string id, HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger)
    {
        logger.LogInformation("Updating study session {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var session = JsonConvert.DeserializeObject<StudySession>(requestBody);

            if (session == null)
            {
                return Results.Json(new { error = "Invalid session data" }, statusCode: StatusCodes.Status400BadRequest);
            }

            var updateErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(session.Category)) updateErrors.Add("Category is required");
            if (session.Hours <= 0) updateErrors.Add("Hours must be greater than zero");
            if (session.Date.Date > DateTime.UtcNow.Date) updateErrors.Add("Date cannot be in the future");
            if (updateErrors.Any())
            {
                return Results.Json(new { error = string.Join("; ", updateErrors) }, statusCode: StatusCodes.Status400BadRequest);
            }

            session.UserId = userId;
            var updatedSession = await studySessionService.UpdateSessionAsync(id, session);

            if (updatedSession == null)
            {
                return Results.Json(new { error = "Study session not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(updatedSession);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating study session {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteStudySession(string id, HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger)
    {
        logger.LogInformation("Deleting study session {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var deleted = await studySessionService.DeleteSessionAsync(id, userId);
            if (!deleted)
            {
                return Results.Json(new { error = "Study session not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting study session {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetStudySessionStats(HttpContext context, IStudySessionService studySessionService, ILogger<Program> logger, DateTime? from, DateTime? to)
    {
        logger.LogInformation("Getting study session stats");

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var stats = await studySessionService.GetStatsAsync(userId, from, to);
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting study session stats");
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
