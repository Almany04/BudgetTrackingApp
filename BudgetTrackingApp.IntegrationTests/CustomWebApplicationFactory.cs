using BudgetTrackingApp.Data;
using Microsoft.AspNetCore.Authentication; // ÚJ
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace BudgetTrackingApp.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Adatbázis csere InMemory-ra (ezt már megírtad)
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetTrackerDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<BudgetTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // 2. ÚJ: Hitelesítés megkerülése
                services.AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }
    }
}