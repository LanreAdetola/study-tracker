using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public class StudyGoalService : IStudyGoalService
{
    private readonly Container _container;
    private readonly Container _sessionsContainer;

    public StudyGoalService(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("study-tracker");
        _container = database.GetContainer("goals");
        _sessionsContainer = database.GetContainer("sessions");
    }

    public async Task<List<StudyGoal>> GetGoalsAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<StudyGoal>(query);
        var results = new List<StudyGoal>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        var hoursByCategory = await GetHoursByCategoryAsync(userId);
        foreach (var goal in results)
        {
            goal.CurrentHours = hoursByCategory.TryGetValue(goal.Name, out var hours) ? hours : 0;
        }

        return results;
    }

    public async Task<StudyGoal?> GetGoalAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<StudyGoal>(id, new PartitionKey(userId));
            var goal = response.Resource;
            goal.CurrentHours = await GetHoursForCategoryAsync(userId, goal.Name);
            return goal;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<StudyGoal> CreateGoalAsync(StudyGoal goal)
    {
        goal.Id = Guid.NewGuid().ToString();
        goal.CreatedAt = DateTime.UtcNow;
        goal.UpdatedAt = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(goal, new PartitionKey(goal.UserId));
        var created = response.Resource;
        created.CurrentHours = await GetHoursForCategoryAsync(created.UserId, created.Name);
        return created;
    }

    public async Task<StudyGoal?> UpdateGoalAsync(string id, StudyGoal goal)
    {
        try
        {
            goal.UpdatedAt = DateTime.UtcNow;
            var response = await _container.ReplaceItemAsync(goal, id, new PartitionKey(goal.UserId));
            var updated = response.Resource;
            updated.CurrentHours = await GetHoursForCategoryAsync(updated.UserId, updated.Name);
            return updated;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    // StudyGoal.CurrentHours is never stored — it's always derived from the sum of
    // StudySession.Hours where the session's category matches the goal's name (this
    // is how SessionForm.razor links a logged session back to a goal).
    private async Task<double> GetHoursForCategoryAsync(string userId, string category)
    {
        var query = new QueryDefinition("SELECT VALUE SUM(c.hours) FROM c WHERE c.userId = @userId AND c.category = @category")
            .WithParameter("@userId", userId)
            .WithParameter("@category", category);

        using var iterator = _sessionsContainer.GetItemQueryIterator<double?>(query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault() ?? 0;
        }

        return 0;
    }

    private async Task<Dictionary<string, double>> GetHoursByCategoryAsync(string userId)
    {
        var query = new QueryDefinition("SELECT c.category, SUM(c.hours) AS totalHours FROM c WHERE c.userId = @userId GROUP BY c.category")
            .WithParameter("@userId", userId);

        using var iterator = _sessionsContainer.GetItemQueryIterator<CategoryHours>(query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        var result = new Dictionary<string, double>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var entry in response)
            {
                result[entry.Category] = entry.TotalHours;
            }
        }

        return result;
    }

    private class CategoryHours
    {
        public string Category { get; set; } = string.Empty;
        public double TotalHours { get; set; }
    }

    public async Task<bool> DeleteGoalAsync(string id, string userId)
    {
        try
        {
            await _container.DeleteItemAsync<StudyGoal>(id, new PartitionKey(userId));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<int> GetGoalCountAsync(string userId)
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<int>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return 0;
    }
}
