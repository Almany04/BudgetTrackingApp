using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Implamentations;
using BudgetTrackingApp.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using BudgetTrackingApp.Api.Components;
using BudgetTrackingApp.Api.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 5. Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<BudgetTrackerDbContext>()
.AddDefaultTokenProviders();

// --- FIX: Cookie Configuration ---
builder.Services.ConfigureApplicationCookie(options =>
{
    // 1. CHANGE THE NAME: This invalidates all previous "Identity.Application" cookies immediately.
    options.Cookie.Name = "BudgetAppSession";

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    // 2. SHORTEN LIFETIME: Set to 60 minutes (or whatever you prefer) instead of 30 days.
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

builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IBudgetLogic, BudgetLogic>();
builder.Services.AddScoped<ICategoryLogic, CategoryLogic>();
builder.Services.AddScoped<ITransactionLogic, TransactionLogic>();
builder.Services.AddScoped<IAiSuggestionLogic, AiSuggestionLogic>();

var app = builder.Build();

// 8. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BudgetTrackingApp.Client._Imports).Assembly);

app.Run();