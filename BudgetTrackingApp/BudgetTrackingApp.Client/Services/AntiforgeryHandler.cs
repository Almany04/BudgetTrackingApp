using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Client.Services
{
    // Ez a handler biztosítja, hogy a Blazor WASM HttpClient
    // elküldje azokat az authentikációs cookie-kat,
    // amiket a szerver beállított bejelentkezéskor.
    // A .NET 8 Antiforgery middleware-nek erre van szüksége.
    public class AntiforgeryHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return base.SendAsync(request, cancellationToken);
        }
    }
}