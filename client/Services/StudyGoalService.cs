using System.Net.Http.Json;
using client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace client.Services
{
    public class StudyGoalService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public StudyGoalService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<List<StudyGoal>> GetUserGoalsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                return new List<StudyGoal>();
            }

            var response = await _httpClient.GetAsync("/api/goals");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<StudyGoal>>() ?? new List<StudyGoal>();
            }

            return new List<StudyGoal>();
        }

        public async Task<StudyGoal?> GetGoalAsync(string id)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var response = await _httpClient.GetAsync($"/api/goals/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StudyGoal>();
            }

            return null;
        }

        public async Task<bool> AddGoalAsync(StudyGoal goal)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var response = await _httpClient.PostAsJsonAsync("/api/goals", goal);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateGoalAsync(StudyGoal goal)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var response = await _httpClient.PutAsJsonAsync($"/api/goals/{goal.Id}", goal);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGoalAsync(string id)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var response = await _httpClient.DeleteAsync($"/api/goals/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<StudyGoal>> GetActiveGoalsAsync()
        {
            var goals = await GetUserGoalsAsync();
            return goals.Where(g => g.IsActive).ToList();
        }
    }
}
