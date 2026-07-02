using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace client.Services;

// Blazor WASM's HttpClient does not send cookies by default. App Service
// Authentication (Easy Auth) relies on a session cookie, so every request to
// our own API must explicitly opt in or the app always looks logged out.
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}
