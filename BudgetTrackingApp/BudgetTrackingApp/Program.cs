using BudgetTrackingApp.Api.Components;
using BudgetTrackingApp.Api.Services;
using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Implamentations;
using BudgetTrackingApp.Repository.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace BudgetTrackingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Blazor szolgáltatások
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddControllers();
            builder.Services.AddRadzenComponents();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddCascadingAuthenticationState();

            // HttpClient regisztrálása a Szerver oldalon (Prerendering miatt fontos!)
            builder.Services.AddScoped(sp => {
                var navigationManager = sp.GetRequiredService<NavigationManager>();
                return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
            });

            // Adatbázis kapcsolat
            builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity beállítások
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<BudgetTrackerDbContext>()
            .AddDefaultTokenProviders();

            // SÜTI BEÁLLÍTÁSOK JAVÍTÁSA (Kritikus a mûködéshez!)
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                // Fejlesztéshez engedjük a HTTP-t is (SameAsRequest), élesben majd HTTPS kell
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax; // Lazább szabályok a navigációhoz
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                // API válaszkódok kezelése átirányítás helyett (SPA barát)
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            // Rétegek regisztrálása (Dependency Injection)
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

            builder.Services.AddScoped<IUserLogic, UserLogic>();
            builder.Services.AddScoped<IBudgetLogic, BudgetLogic>();
            builder.Services.AddScoped<ICategoryLogic, CategoryLogic>();
            builder.Services.AddScoped<ITransactionLogic, TransactionLogic>();

            builder.Services.AddAntiforgery();

            var app = builder.Build();

            // Pipeline konfiguráció
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(BudgetTrackingApp.Client._Imports).Assembly);

            app.Run();
        }
    }
}