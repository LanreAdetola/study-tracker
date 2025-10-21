using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Configure Cosmos DB client
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("CosmosDBConnectionString environment variable is required");
            }
            return new CosmosClient(connectionString);
        });
        
        // Add study session service
        services.AddScoped<IStudySessionService, StudySessionService>();
    })
    .Build();

host.Run();