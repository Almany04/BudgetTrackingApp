using BudgetTrackingApp.Api.Components;
using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Implamentations;
using BudgetTrackingApp.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Security.Claims; // Ezt az importot is hozzáadtam a biztonság kedvéért

namespace BudgetTrackingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ------ AUTHENTIKÁCIÓS SZOLGÁLTATÁSOK ------
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<BudgetTrackerDbContext>();

            // Autorizáció regisztrálása
            builder.Services.AddAuthorization();

            // Cookie beállítások API-hoz (401-es hiba átirányítás helyett)
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403; // Forbidden
                    return Task.CompletedTask;
                };
            });
            // ------ AUTHENTIKÁCIÓ VÉGE ------

            // ------ SZERVIZEK ÉS REPOZITÓRIUMOK ------
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICategoryLogic, CategoryLogic>();
            builder.Services.AddScoped<ITransactionLogic, TransactionLogic>();
            builder.Services.AddScoped<IBudgetLogic, BudgetLogic>();
            builder.Services.AddScoped<IUserLogic, UserLogic>();

            // ------ UI ÉS API SZOLGÁLTATÁSOK ------
            builder.Services.AddMudServices(); // MudBlazor szervizek

            // API Kontrollerek szervizeinek regisztrálása (EZ VOLT A HIBA)
            builder.Services.AddControllers();

            // Blazor komponensek regisztrálása
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            // --------------------------------------------------
            //          SZOLGÁLTATÁSOK REGISZTRÁLÁSA VÉGE
            // --------------------------------------------------

            var app = builder.Build();

            // --------------------------------------------------
            //          HTTP KÉRÉS PIPELINE BEÁLLÍTÁSA
            // --------------------------------------------------

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            // Fontos a sorrend!
            app.UseAuthentication();
            app.UseAuthorization();

            // Blazor komponensek "bekötése"
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            // API Kontrollerek "bekötése" (ez keresi az /api/user/login-t stb.)
            app.MapControllers();

            app.Run();
        }
    }
}