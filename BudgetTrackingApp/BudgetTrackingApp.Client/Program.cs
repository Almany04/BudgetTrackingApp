using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using BudgetTrackingApp.Client.Services;
using Microsoft.AspNetCore.Components.Authorization; 
using Microsoft.AspNetCore.Components; 

namespace BudgetTrackingApp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            
            builder.Services.AddTransient<AntiforgeryHandler>();

            
            builder.Services.AddHttpClient("Api", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
            .AddHttpMessageHandler<AntiforgeryHandler>();

           
            builder.Services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("Api");
            });

            // ------ SZÜKSÉGES KLIENS SZERVIZEK ------

            builder.Services.AddMudServices();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthenticationStateProvider>());

            // ------ SZERVIZEK VÉGE ------

            await builder.Build().RunAsync();
        }
    }
}