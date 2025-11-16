using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BudgetTrackingApp.Data.Entities;

namespace BudgetTrackingApp.Api.Services
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServerAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}