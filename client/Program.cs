using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using client.Services;
using client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add authorization services
builder.Services.AddAuthorizationCore();

// Add Azure Static Web Apps authentication state provider
builder.Services.AddScoped<AuthenticationStateProvider, AzureStaticWebAppsAuthenticationStateProvider>();

builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<StudySessionService>();
builder.Services.AddScoped<StudyGoalService>();
builder.Services.AddScoped<UserService>();

await builder.Build().RunAsync();
