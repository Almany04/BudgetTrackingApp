using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BudgetTrackingApp.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen; 

namespace BudgetTrackingApp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthenticationStateProvider>());

           
            builder.Services.AddRadzenComponents();

            builder.Services.AddTransient<AntiforgeryHandler>();
            builder.Services.AddScoped(sp =>
            {
                var handler = sp.GetRequiredService<AntiforgeryHandler>();
                handler.InnerHandler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                };
                return httpClient;
            });

            await builder.Build().RunAsync();
        }
    }
}