using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BudgetTrackingApp.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BudgetTrackingApp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();

            // Register the Custom Auth Provider
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthenticationStateProvider>());

            builder.Services.AddRadzenComponents();

           
            builder.Services.AddTransient<CookieHandler>();

            
            builder.Services.AddScoped(sp =>
            {
                var handler = sp.GetRequiredService<CookieHandler>();
                handler.InnerHandler = new HttpClientHandler();

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                };
                return client;
            });

            await builder.Build().RunAsync();
        }
    }
}