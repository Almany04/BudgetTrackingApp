using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services; // 1. MudBlazor importálása
using BudgetTrackingApp.Client.Services; // 2. A saját Auth provider importálása
using Microsoft.AspNetCore.Components.Authorization; // 3. Az Auth rendszer importálása

namespace BudgetTrackingApp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

           
            builder.Services.AddMudServices();

           
            builder.Services.AddAuthorizationCore();

            
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();

            
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthenticationStateProvider>());


            await builder.Build().RunAsync();
        }
    }
}