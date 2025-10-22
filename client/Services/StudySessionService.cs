using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Authorization;
using client.Models;

namespace client.Services
{
    public class StudySessionService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public StudySessionService(
            HttpClient httpClient, 
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        /// <summary>
        /// Gets study sessions for the currently authenticated user
        /// </summary>
        public async Task<List<StudySession>> GetUserSessionsAsync()
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return new List<StudySession>();
                }

                // Azure Functions API will filter by authenticated user automatically
                var sessions = await _httpClient.GetFromJsonAsync<List<StudySession>>("/api/sessions");
                return sessions ?? new List<StudySession>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sessions: {ex.Message}");
                return new List<StudySession>();
            }
        }

        /// <summary>
        /// Adds a new study session for the authenticated user
        /// </summary>
        public async Task<bool> AddSessionAsync(StudySession session)
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return false;
                }

                var response = await _httpClient.PostAsJsonAsync("/api/sessions", session);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding session: {ex.Message}");
                return false;
            }
        }
    }
}
