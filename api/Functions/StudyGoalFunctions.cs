using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudyTracker.Api.Models;
using StudyTracker.Api.Services;
using System.Net;
using Newtonsoft.Json;

namespace StudyTracker.Api.Functions;

public class StudyGoalFunctions
{
    private readonly ILogger<StudyGoalFunctions> _logger;
    private readonly IStudyGoalService _studyGoalService;
    private const int MAX_GOALS = 5;

    public StudyGoalFunctions(ILogger<StudyGoalFunctions> logger, IStudyGoalService studyGoalService)
    {
        _logger = logger;
        _studyGoalService = studyGoalService;
    }

    [Function("GetStudyGoals")]
    public async Task<HttpResponseData> GetStudyGoals(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "goals")] HttpRequestData req)
    {
        _logger.LogInformation("Getting study goals");

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var goals = await _studyGoalService.GetGoalsAsync(userId);
            return await CreateJsonResponse(req, goals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study goals");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("GetStudyGoal")]
    public async Task<HttpResponseData> GetStudyGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "goals/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Getting study goal {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var goal = await _studyGoalService.GetGoalAsync(id, userId);
            if (goal == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study goal not found");
            }

            return await CreateJsonResponse(req, goal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study goal {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("CreateStudyGoal")]
    public async Task<HttpResponseData> CreateStudyGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "goals")] HttpRequestData req)
    {
        _logger.LogInformation("Creating study goal");

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            // Check goal count limit (5 goals max)
            var goalCount = await _studyGoalService.GetGoalCountAsync(userId);
            if (goalCount >= MAX_GOALS)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Maximum of {MAX_GOALS} goals allowed. Please delete an existing goal before creating a new one.");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var goal = JsonConvert.DeserializeObject<StudyGoal>(requestBody);

            if (goal == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid goal data");
            }

            // Server-side validation
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(goal.Name)) validationErrors.Add("Goal name is required");
            if (string.IsNullOrWhiteSpace(goal.Type)) validationErrors.Add("Type is required");
            if (goal.Type != "Subject" && goal.Type != "Certification") 
                validationErrors.Add("Type must be either 'Subject' or 'Certification'");
            if (goal.TargetHours <= 0) validationErrors.Add("Target hours must be greater than zero");
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, string.Join("; ", validationErrors));
            }

            goal.UserId = userId;
            var createdGoal = await _studyGoalService.CreateGoalAsync(goal);
            
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(createdGoal);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating study goal");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("UpdateStudyGoal")]
    public async Task<HttpResponseData> UpdateStudyGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "goals/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Updating study goal {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var goal = JsonConvert.DeserializeObject<StudyGoal>(requestBody);

            if (goal == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid goal data");
            }

            // Server-side validation
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(goal.Name)) validationErrors.Add("Goal name is required");
            if (string.IsNullOrWhiteSpace(goal.Type)) validationErrors.Add("Type is required");
            if (goal.Type != "Subject" && goal.Type != "Certification") 
                validationErrors.Add("Type must be either 'Subject' or 'Certification'");
            if (goal.TargetHours <= 0) validationErrors.Add("Target hours must be greater than zero");
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, string.Join("; ", validationErrors));
            }

            goal.UserId = userId;
            var updatedGoal = await _studyGoalService.UpdateGoalAsync(id, goal);
            
            if (updatedGoal == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study goal not found");
            }

            return await CreateJsonResponse(req, updatedGoal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating study goal {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    [Function("DeleteStudyGoal")]
    public async Task<HttpResponseData> DeleteStudyGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "goals/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Deleting study goal {Id}", id);

        try
        {
            var userId = GetUserIdFromRequest(req);
            if (string.IsNullOrEmpty(userId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
            }

            var deleted = await _studyGoalService.DeleteGoalAsync(id, userId);
            if (!deleted)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Study goal not found");
            }

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting study goal {Id}", id);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static string? GetUserIdFromRequest(HttpRequestData req)
    {
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
