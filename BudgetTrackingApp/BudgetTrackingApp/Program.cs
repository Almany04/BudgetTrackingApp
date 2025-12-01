using BudgetTrackingApp.Api.Components;
using BudgetTrackingApp.Api.Services;
using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Implamentations;
using BudgetTrackingApp.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpContextAccessor();

// 2. Register Server Authentication
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// 3. Register HttpClient
builder.Services.AddScoped(sp =>
{
    var navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});

// 4. Database
builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
    options.UseSqlite("Data Source=budget.db"));

// 5. Identity Configuration - SECURITY ENHANCED
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // SECURITY FIX: Enforce special chars
    options.Password.RequiredLength = 10; // SECURITY FIX: Increase length
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // SECURITY FIX: Brute force protection
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<BudgetTrackerDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BudgetAppSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Strong CSRF protection
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// 7. Register Dependencies
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ISavingGoalRepository, SavingGoalRepository>();

builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IBudgetLogic, BudgetLogic>();
builder.Services.AddScoped<ICategoryLogic, CategoryLogic>();
builder.Services.AddScoped<ITransactionLogic, TransactionLogic>();
builder.Services.AddScoped<IAiSuggestionLogic, AiSuggestionLogic>();
builder.Services.AddScoped<ISavingGoalLogic, SavingGoalLogic>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("General", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("Strict", options =>
    {
        options.PermitLimit = 10; // Login/Register limit
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

var app = builder.Build();

// SECURITY FIX: Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    // context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; ..."); // Needs careful tuning for Blazor/Radzen
    await next();
});

app.UseRateLimiter();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BudgetTrackerDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection(); // Ensure HTTPS
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BudgetTrackingApp.Client._Imports).Assembly);

app.Run();

public partial class Program { }