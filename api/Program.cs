using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Add Cosmos DB client
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString") 
                ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            return new CosmosClient(connectionString);
        });
        
        // Add services here
        services.AddScoped<IStudySessionService, StudySessionService>();
    })
    .Build();

host.Run();