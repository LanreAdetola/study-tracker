using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Models;

namespace StudyTracker.Api.Services;

public class StudyGoalService : IStudyGoalService
{
    private readonly Container _container;

    public StudyGoalService(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("study-tracker");
        _container = database.GetContainer("goals");
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

        return results;
    }

    public async Task<StudyGoal?> GetGoalAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<StudyGoal>(id, new PartitionKey(userId));
            return response.Resource;
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
        return response.Resource;
    }

    public async Task<StudyGoal?> UpdateGoalAsync(string id, StudyGoal goal)
    {
        try
        {
            goal.UpdatedAt = DateTime.UtcNow;
            var response = await _container.ReplaceItemAsync(goal, id, new PartitionKey(goal.UserId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
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
