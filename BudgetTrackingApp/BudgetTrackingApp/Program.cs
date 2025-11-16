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
using MudBlazor.Services;
using System.Security.Claims;

namespace BudgetTrackingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddControllers();
            builder.Services.AddMudServices();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddCascadingAuthenticationState();

            builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
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