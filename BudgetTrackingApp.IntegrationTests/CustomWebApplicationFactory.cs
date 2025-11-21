using BudgetTrackingApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Linq;

namespace BudgetTrackingApp.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Keressük meg és TÁVOLÍTSUK EL a valódi adatbázis beállítást (SQL Server/SQLite)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BudgetTrackerDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Helyette adjunk hozzá IN-MEMORY adatbázist a teszthez
                services.AddDbContext<BudgetTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        }
    }
}