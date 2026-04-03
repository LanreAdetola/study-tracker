using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StudyTracker.Api.Services;

public class StudySessionService : IStudySessionService
{
    private readonly Container _container;
    private readonly string _databaseName;
    private readonly string _containerName;

    public StudySessionService(CosmosClient cosmosClient)
    {
        _databaseName = Environment.GetEnvironmentVariable("CosmosDBDatabaseName") ?? "study-tracker";
        _containerName = Environment.GetEnvironmentVariable("CosmosDBContainerName") ?? "sessions";
        _container = cosmosClient.GetContainer(_databaseName, _containerName);
    }

    public async Task<IEnumerable<StudySession>> GetSessionsAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId ORDER BY c.date DESC")
            .WithParameter("@userId", userId);
        
        var results = new List<StudySession>();
        using var feed = _container.GetItemQueryIterator<StudySession>(query);
        
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            results.AddRange(response);
        }
        
        return results;
    }

    public async Task<StudySession?> GetSessionAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<StudySession>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<StudySession> CreateSessionAsync(StudySession session)
    {
        session.Id = Guid.NewGuid().ToString();
        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(session, new PartitionKey(session.UserId));
        return response.Resource;
    }

    public async Task<StudySession?> UpdateSessionAsync(string id, StudySession session)
    {
        try
        {
            session.Id = id;
            session.UpdatedAt = DateTime.UtcNow;
            
            var response = await _container.UpsertItemAsync(session, new PartitionKey(session.UserId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteSessionAsync(string id, string userId)
    {
        try
        {
            await _container.DeleteItemAsync<StudySession>(id, new PartitionKey(userId));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}