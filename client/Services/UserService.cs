using System.Net.Http.Json;
using client.Models;

namespace client.Services;

public class UserService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "/api/users";

    public UserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UserCountResponse?> GetUserCountAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<UserCountResponse>($"{BaseUrl}/count");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserProfile?> RegisterUserAsync()
    {
        try
        {
            var response = await _http.PostAsync($"{BaseUrl}/register", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // App is at capacity
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorMessage);
            }
            
            return null;
        }
        catch (InvalidOperationException)
        {
            // Re-throw capacity errors
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<UserProfile>($"{BaseUrl}/me");
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class UserCountResponse
{
    public int Count { get; set; }
    public int MaxUsers { get; set; }
    public bool CanRegister { get; set; }
}
