using Microsoft.Azure.Cosmos;
using StudyTracker.Api.Endpoints;
using StudyTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    string connStr = config["CosmosDBConnectionString"]
        ?? throw new InvalidOperationException("CosmosDBConnectionString configuration is required");
    return new CosmosClient(connStr);
});

builder.Services.AddScoped<IStudySessionService, StudySessionService>();
builder.Services.AddScoped<IStudyGoalService, StudyGoalService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.MapAuthEndpoints();
app.MapStudySessionEndpoints();
app.MapStudyGoalEndpoints();
app.MapUserProfileEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
