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

        AuthMeClientPrincipal? result = principal == null
            ? null
            : new AuthMeClientPrincipal
            {
                IdentityProvider = principal.IdentityProvider ?? "",
                UserId = principal.UserId ?? "",
                UserDetails = principal.UserDetails ?? "",
                Claims = (principal.Claims ?? Enumerable.Empty<ClientPrincipalClaim>())
                    .Select(c => new AuthMeClaim { Type = c.Type ?? "", Value = c.Value ?? "" })
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
