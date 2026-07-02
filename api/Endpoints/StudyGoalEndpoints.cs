using StudyTracker.Api.Auth;
using StudyTracker.Api.Models;
using StudyTracker.Api.Services;
using Newtonsoft.Json;

namespace StudyTracker.Api.Endpoints;

public static class StudyGoalEndpoints
{
    private const int MaxGoals = 5;

    public static void MapStudyGoalEndpoints(this WebApplication app)
    {
        app.MapGet("/api/goals", GetStudyGoals);
        app.MapGet("/api/goals/{id}", GetStudyGoal);
        app.MapPost("/api/goals", CreateStudyGoal);
        app.MapPut("/api/goals/{id}", UpdateStudyGoal);
        app.MapDelete("/api/goals/{id}", DeleteStudyGoal);
    }

    private static async Task<IResult> GetStudyGoals(HttpContext context, IStudyGoalService studyGoalService, ILogger<Program> logger)
    {
        logger.LogInformation("Getting study goals");

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var goals = await studyGoalService.GetGoalsAsync(userId);
            return Results.Ok(goals);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting study goals");
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetStudyGoal(string id, HttpContext context, IStudyGoalService studyGoalService, ILogger<Program> logger)
    {
        logger.LogInformation("Getting study goal {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var goal = await studyGoalService.GetGoalAsync(id, userId);
            if (goal == null)
            {
                return Results.Json(new { error = "Study goal not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(goal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting study goal {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CreateStudyGoal(HttpContext context, IStudyGoalService studyGoalService, ILogger<Program> logger)
    {
        logger.LogInformation("Creating study goal");

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var goalCount = await studyGoalService.GetGoalCountAsync(userId);
            if (goalCount >= MaxGoals)
            {
                return Results.Json(
                    new { error = $"Maximum of {MaxGoals} goals allowed. Please delete an existing goal before creating a new one." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var goal = JsonConvert.DeserializeObject<StudyGoal>(requestBody);

            if (goal == null)
            {
                return Results.Json(new { error = "Invalid goal data" }, statusCode: StatusCodes.Status400BadRequest);
            }

            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(goal.Name)) validationErrors.Add("Goal name is required");
            if (string.IsNullOrWhiteSpace(goal.Type)) validationErrors.Add("Type is required");
            if (goal.Type != "Subject" && goal.Type != "Certification")
                validationErrors.Add("Type must be either 'Subject' or 'Certification'");
            if (goal.TargetHours <= 0) validationErrors.Add("Target hours must be greater than zero");
            if (validationErrors.Any())
            {
                return Results.Json(new { error = string.Join("; ", validationErrors) }, statusCode: StatusCodes.Status400BadRequest);
            }

            goal.UserId = userId;
            var createdGoal = await studyGoalService.CreateGoalAsync(goal);

            return Results.Json(createdGoal, statusCode: StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating study goal");
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateStudyGoal(string id, HttpContext context, IStudyGoalService studyGoalService, ILogger<Program> logger)
    {
        logger.LogInformation("Updating study goal {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var goal = JsonConvert.DeserializeObject<StudyGoal>(requestBody);

            if (goal == null)
            {
                return Results.Json(new { error = "Invalid goal data" }, statusCode: StatusCodes.Status400BadRequest);
            }

            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(goal.Name)) validationErrors.Add("Goal name is required");
            if (string.IsNullOrWhiteSpace(goal.Type)) validationErrors.Add("Type is required");
            if (goal.Type != "Subject" && goal.Type != "Certification")
                validationErrors.Add("Type must be either 'Subject' or 'Certification'");
            if (goal.TargetHours <= 0) validationErrors.Add("Target hours must be greater than zero");
            if (validationErrors.Any())
            {
                return Results.Json(new { error = string.Join("; ", validationErrors) }, statusCode: StatusCodes.Status400BadRequest);
            }

            goal.UserId = userId;
            var updatedGoal = await studyGoalService.UpdateGoalAsync(id, goal);

            if (updatedGoal == null)
            {
                return Results.Json(new { error = "Study goal not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(updatedGoal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating study goal {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteStudyGoal(string id, HttpContext context, IStudyGoalService studyGoalService, ILogger<Program> logger)
    {
        logger.LogInformation("Deleting study goal {Id}", id);

        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { error = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var deleted = await studyGoalService.DeleteGoalAsync(id, userId);
            if (!deleted)
            {
                return Results.Json(new { error = "Study goal not found" }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting study goal {Id}", id);
            return Results.Json(new { error = "Internal server error" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
