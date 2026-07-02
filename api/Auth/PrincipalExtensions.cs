using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyTracker.Api.Auth;

// Unifies identity extraction from App Service's built-in authentication ("Easy Auth") headers.
// App Service injects the same X-MS-CLIENT-PRINCIPAL-ID / X-MS-CLIENT-PRINCIPAL headers that
// Azure Static Web Apps did, so this mirrors the previous SWA-era parsing logic.
public static class PrincipalExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.Request.Headers.TryGetValue("x-ms-client-principal-id", out var values)
            ? values.FirstOrDefault()
            : null;
    }

    public static ClientPrincipal? GetClientPrincipal(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("x-ms-client-principal", out var values))
        {
            return null;
        }

        var data = values.FirstOrDefault();
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        var decoded = Convert.FromBase64String(data);
        var json = System.Text.Encoding.UTF8.GetString(decoded);
        return JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        if (!string.IsNullOrEmpty(UserId))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserId));
        }

        return new ClaimsPrincipal(identity);
    }
}

public class ClientPrincipalClaim
{
    // Wire format uses the short "typ"/"val" names (matches the Claim serialization
    // contract App Service and Static Web Apps both use), not "type"/"value".
    [JsonPropertyName("typ")]
    public string? Type { get; set; }

    [JsonPropertyName("val")]
    public string? Value { get; set; }
}
