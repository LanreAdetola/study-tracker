using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudyTracker.Api.Services;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace StudyTracker.Api.Functions;

public class UserProfileFunctions
{
    private readonly ILogger<UserProfileFunctions> _logger;
    private readonly IUserProfileService _userProfileService;

    public UserProfileFunctions(ILogger<UserProfileFunctions> logger, IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }

    [Function("GetUserCount")]
    public async Task<HttpResponseData> GetUserCount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/count")] HttpRequestData req)
    {
        try
        {
            var count = await _userProfileService.GetUserCountAsync();
            var canRegister = await _userProfileService.CanRegisterAsync();

            var result = new
            {
                count = count,
                maxUsers = 50,
                canRegister = canRegister
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving user count");
            return response;
        }
    }

    [Function("RegisterUser")]
    public async Task<HttpResponseData> RegisterUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/register")] HttpRequestData req)
    {
        try
        {
            // Get user ID from claims
            var principal = req.GetPrincipal();
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("User must be authenticated");
                return unauthorizedResponse;
            }

            // Get display name and email from claims
            var displayName = principal?.FindFirst(ClaimTypes.Name)?.Value 
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
                ?? "User";
            
            var email = principal?.FindFirst(ClaimTypes.Email)?.Value 
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            // Attempt to register or get existing user
            var userProfile = await _userProfileService.GetOrCreateUserAsync(userId, displayName, email);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userProfile);
            return response;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("capacity"))
        {
            // User limit reached
            var response = req.CreateResponse(HttpStatusCode.Forbidden);
            await response.WriteStringAsync(ex.Message);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred during user registration");
            return response;
        }
    }

    [Function("GetCurrentUser")]
    public async Task<HttpResponseData> GetCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/me")] HttpRequestData req)
    {
        try
        {
            // Get user ID from claims
            var principal = req.GetPrincipal();
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("User must be authenticated");
                return unauthorizedResponse;
            }

            var userProfile = await _userProfileService.GetUserProfileAsync(userId);
            
            if (userProfile == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("User profile not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userProfile);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving user profile");
            return response;
        }
    }
}

// Extension method to get ClaimsPrincipal from HttpRequestData
public static class HttpRequestDataExtensions
{
    public static ClaimsPrincipal? GetPrincipal(this HttpRequestData req)
    {
        var principal = req.FunctionContext.Items.TryGetValue("ClaimsPrincipal", out var obj) 
            ? obj as ClaimsPrincipal 
            : null;

        // Try to get from headers if not in context
        if (principal == null && req.Headers.TryGetValues("X-MS-CLIENT-PRINCIPAL", out var headerValues))
        {
            var data = headerValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(data))
            {
                var decoded = Convert.FromBase64String(data);
                var json = System.Text.Encoding.UTF8.GetString(decoded);
                var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (clientPrincipal != null)
                {
                    principal = clientPrincipal.ToClaimsPrincipal();
                }
            }
        }

        return principal;
    }
}

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public IEnumerable<ClientPrincipalClaim>? Claims { get; set; }

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var identity = new ClaimsIdentity(IdentityProvider);
        
        if (Claims != null)
        {
            identity.AddClaims(Claims.Select(c => new Claim(c.Type ?? "", c.Value ?? "")));
        }
        
        // Add userId as NameIdentifier if available
        if (!string.IsNullOrEmpty(UserId))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserId));
        }

        return new ClaimsPrincipal(identity);
    }
}

public class ClientPrincipalClaim
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}
