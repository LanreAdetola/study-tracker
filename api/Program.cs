using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // ✅ Add Application Insights
        services.AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights();

        // ✅ Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            string connStr = config["CosmosDBConnectionString"] 
                ?? throw new InvalidOperationException("CosmosDBConnectionString configuration is required");
            return new CosmosClient(connStr);
        });

        // ✅ Register StudySession service
        services.AddScoped<IStudySessionService, StudySessionService>();
        
        // ✅ Register StudyGoal service
        services.AddScoped<IStudyGoalService, StudyGoalService>();
    })
    .Build();

host.Run();