using System.Security.Claims;
using StudyTracker.Api.Auth;
using StudyTracker.Api.Services;

namespace StudyTracker.Api.Endpoints;

public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users/count", GetUserCount);
        app.MapPost("/api/users/register", RegisterUser);
        app.MapGet("/api/users/me", GetCurrentUser);
    }

    private static async Task<IResult> GetUserCount(IUserProfileService userProfileService, ILogger<Program> logger)
    {
        try
        {
            var count = await userProfileService.GetUserCountAsync();
            var canRegister = await userProfileService.CanRegisterAsync();

            return Results.Ok(new
            {
                count,
                maxUsers = 50,
                canRegister
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user count");
            return Results.Text("An error occurred while retrieving user count", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> RegisterUser(HttpContext context, IUserProfileService userProfileService, ILogger<Program> logger)
    {
        try
        {
            var principal = context.GetClientPrincipal()?.ToClaimsPrincipal();
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                ?? context.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Text("User must be authenticated", statusCode: StatusCodes.Status401Unauthorized);
            }

            var displayName = principal?.FindFirst(ClaimTypes.Name)?.Value
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                ?? "User";

            var email = principal?.FindFirst(ClaimTypes.Email)?.Value
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            var userProfile = await userProfileService.GetOrCreateUserAsync(userId, displayName, email);
            return Results.Ok(userProfile);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("capacity"))
        {
            return Results.Text(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering user");
            return Results.Text("An error occurred during user registration", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetCurrentUser(HttpContext context, IUserProfileService userProfileService, ILogger<Program> logger)
    {
        try
        {
            var principal = context.GetClientPrincipal()?.ToClaimsPrincipal();
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                ?? context.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Text("User must be authenticated", statusCode: StatusCodes.Status401Unauthorized);
            }

            var userProfile = await userProfileService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return Results.Text("User profile not found", statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(userProfile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting current user");
            return Results.Text("An error occurred while retrieving user profile", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
