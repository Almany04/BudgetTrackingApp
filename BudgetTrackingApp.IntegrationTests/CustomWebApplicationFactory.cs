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
                // 1. Adatbázis csere (ez már jó volt)
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetTrackerDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<BudgetTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // 2. JAVÍTOTT AUTHENTICATION MOCKOLÁS
                // Ez a trükk: Felülírjuk az alapértelmezett Scheme-et "TestScheme"-re!
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }
    }
}