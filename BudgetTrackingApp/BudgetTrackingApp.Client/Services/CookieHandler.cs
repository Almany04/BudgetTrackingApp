using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BudgetTrackingApp.Client.Services
{
    public class CookieHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // CRITICAL FIX: This tells the browser to include cookies (credentials) in the request.
            // Without this, the server will never know who you are after you login.
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return base.SendAsync(request, cancellationToken);
        }
    }
}