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

    public async Task<StudySessionStats> GetStatsAsync(string userId, DateTime? from, DateTime? to)
    {
        var startDate = from ?? DateTime.UtcNow.Date.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow.Date;

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.date >= @startDate AND c.date <= @endDate AND c.hours > 0")
            .WithParameter("@userId", userId)
            .WithParameter("@startDate", startDate)
            .WithParameter("@endDate", endDate.AddDays(1));

        var sessions = new List<StudySession>();
        using var feed = _container.GetItemQueryIterator<StudySession>(query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            sessions.AddRange(response);
        }

        // Filter out future dates
        var now = DateTime.UtcNow.Date;
        var validSessions = sessions.Where(s => s.Date.Date <= now).ToList();

        var totalDays = (endDate - startDate).Days + 1;
        var totalHours = validSessions.Sum(s => s.Hours);

        // Build daily breakdown with zero-fill
        var dailyMap = validSessions
            .GroupBy(s => s.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Hours));

        var dailyBreakdown = new List<DailyHours>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            dailyBreakdown.Add(new DailyHours
            {
                Date = date,
                Hours = dailyMap.TryGetValue(date, out var hours) ? hours : 0
            });
        }

        // Build category breakdown
        var hoursByCategory = validSessions
            .GroupBy(s => s.Category ?? "Uncategorized")
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Hours));

        // Compute streak and weekly hours from ALL sessions (not just date range)
        var allSessionsQuery = new QueryDefinition(
            "SELECT c.date, c.hours FROM c WHERE c.userId = @userId AND c.hours > 0")
            .WithParameter("@userId", userId);

        var allSessions = new List<StudySession>();
        using var allFeed = _container.GetItemQueryIterator<StudySession>(allSessionsQuery,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        while (allFeed.HasMoreResults)
        {
            var resp = await allFeed.ReadNextAsync();
            allSessions.AddRange(resp);
        }

        var allValidSessions = allSessions.Where(s => s.Date.Date <= now).ToList();

        // Streak calculation
        var studyDates = allValidSessions
            .Select(s => s.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int currentStreak = 0;
        int longestStreak = 0;

        if (studyDates.Any())
        {
            // Current streak: count backward from today (or yesterday)
            var checkDate = now;
            if (!studyDates.Contains(checkDate))
                checkDate = now.AddDays(-1);

            if (studyDates.Contains(checkDate))
            {
                currentStreak = 1;
                var prev = checkDate.AddDays(-1);
                while (studyDates.Contains(prev))
                {
                    currentStreak++;
                    prev = prev.AddDays(-1);
                }
            }

            // Longest streak: scan all dates ascending
            var ascending = studyDates.OrderBy(d => d).ToList();
            int run = 1;
            longestStreak = 1;
            for (int i = 1; i < ascending.Count; i++)
            {
                if ((ascending[i] - ascending[i - 1]).Days == 1)
                {
                    run++;
                    if (run > longestStreak) longestStreak = run;
                }
                else
                {
                    run = 1;
                }
            }
        }

        // Weekly hours (ISO: Monday-Sunday)
        var todayDow = (int)now.DayOfWeek;
        var mondayOffset = todayDow == 0 ? -6 : -(todayDow - 1);
        var thisWeekStart = now.AddDays(mondayOffset);
        var lastWeekStart = thisWeekStart.AddDays(-7);

        var thisWeekHours = allValidSessions
            .Where(s => s.Date.Date >= thisWeekStart && s.Date.Date < thisWeekStart.AddDays(7))
            .Sum(s => s.Hours);

        var lastWeekHours = allValidSessions
            .Where(s => s.Date.Date >= lastWeekStart && s.Date.Date < thisWeekStart)
            .Sum(s => s.Hours);

        return new StudySessionStats
        {
            TotalSessions = validSessions.Count,
            TotalHours = Math.Round(totalHours, 1),
            AverageHoursPerDay = totalDays > 0 ? Math.Round(totalHours / totalDays, 1) : 0,
            HoursByCategory = hoursByCategory,
            DailyBreakdown = dailyBreakdown,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            ThisWeekHours = Math.Round(thisWeekHours, 1),
            LastWeekHours = Math.Round(lastWeekHours, 1)
        };
    }
}