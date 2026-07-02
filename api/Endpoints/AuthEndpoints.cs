using System.Text.Json.Serialization;
using StudyTracker.Api.Auth;

namespace StudyTracker.Api.Endpoints;

// Reproduces the shape of Azure Static Web Apps' /.auth/me endpoint (a wrapped
// clientPrincipal object) from App Service's raw auth headers. App Service's own
// /.auth/me returns a different, token-store-shaped response, so the client's
// authentication state provider calls this endpoint instead.
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/me", GetCurrentPrincipal);
    }

    private static IResult GetCurrentPrincipal(HttpContext context)
    {
        var principal = context.GetClientPrincipal();
        var headerUserId = context.GetUserId();

        if (principal == null && string.IsNullOrEmpty(headerUserId))
        {
            return Results.Ok(new AuthMeResponse { ClientPrincipal = null });
        }

        var claims = (principal?.Claims ?? Enumerable.Empty<ClientPrincipalClaim>()).ToList();

        // Azure App Service's real X-MS-CLIENT-PRINCIPAL only reliably populates the
        // claims array — identityProvider/userId/userDetails often come back empty.
        // An empty identityProvider is fatal client-side: ClaimsIdentity treats an
        // empty authenticationType as NOT authenticated. Always fall back to a
        // non-empty value derived from claims (or the simple id header) instead.
        var identityProvider = !string.IsNullOrEmpty(principal?.IdentityProvider)
            ? principal!.IdentityProvider!
            : claims.Any(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid") ? "aad" : "appservice";

        var userId = !string.IsNullOrEmpty(principal?.UserId)
            ? principal!.UserId!
            : headerUserId
                ?? claims.FirstOrDefault(c => c.Type is "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" or "sub")?.Value
                ?? "";

        var userDetails = !string.IsNullOrEmpty(principal?.UserDetails)
            ? principal!.UserDetails!
            : claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "login")?.Value
                ?? "";

        var result = new AuthMeClientPrincipal
        {
            IdentityProvider = identityProvider,
            UserId = userId,
            UserDetails = userDetails,
            Claims = claims.Select(c => new AuthMeClaim { Type = c.Type ?? "", Value = c.Value ?? "" })
        };

        return Results.Ok(new AuthMeResponse { ClientPrincipal = result });
    }
}

public class AuthMeResponse
{
    [JsonPropertyName("clientPrincipal")]
    public AuthMeClientPrincipal? ClientPrincipal { get; set; }
}

public class AuthMeClientPrincipal
{
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; } = "";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("userDetails")]
    public string UserDetails { get; set; } = "";

    [JsonPropertyName("claims")]
    public IEnumerable<AuthMeClaim> Claims { get; set; } = Enumerable.Empty<AuthMeClaim>();
}

public class AuthMeClaim
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}
