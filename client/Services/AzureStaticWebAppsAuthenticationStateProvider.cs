using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace client.Services;

public class AzureStaticWebAppsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public AzureStaticWebAppsAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userInfo = await _httpClient.GetFromJsonAsync<UserInfo>("/.auth/me");
            
            if (userInfo?.ClientPrincipal != null)
            {
                var claims = userInfo.ClientPrincipal.Claims.Select(c => new Claim(c.Type, c.Value));
                var identity = new ClaimsIdentity(claims, userInfo.ClientPrincipal.IdentityProvider);
                var user = new ClaimsPrincipal(identity);
                
                return new AuthenticationState(user);
            }
        }
        catch
        {
            // If there's an error, assume the user is not authenticated
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}

public class UserInfo
{
    public ClientPrincipal? ClientPrincipal { get; set; }
}

public class ClientPrincipal
{
    public string IdentityProvider { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserDetails { get; set; } = "";
    public IEnumerable<UserClaim> Claims { get; set; } = new List<UserClaim>();
}

public class UserClaim
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}