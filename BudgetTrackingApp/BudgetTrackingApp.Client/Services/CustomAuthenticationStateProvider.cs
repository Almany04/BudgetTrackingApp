using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Net.Http.Json;

namespace BudgetTrackingApp.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        // Initialize with anonymous (not logged in) state
        private readonly AuthenticationState _anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Always ask the server: "Am I logged in?" 
                // This keeps Client and Server in sync via the HTTP-Only Cookie.
                var userDto = await _httpClient.GetFromJsonAsync<UserLoginResponseDto>("api/user/current");

                if (userDto != null && !string.IsNullOrEmpty(userDto.UserId))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                        new Claim(ClaimTypes.Email, userDto.Email)
                    };

                    var identity = new ClaimsIdentity(claims, "ServerAuth");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            catch
            {
                // If server returns 401 or error, assume logged out.
            }

            return _anonymousState;
        }

        // Called by Login.razor to update UI immediately
        public void NotifyUserLogin(string userId, string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };
            var identity = new ClaimsIdentity(claims, "ServerAuth");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        // Called by Logout.razor - This fixes your compiler error
        public Task LogoutAsync()
        {
            // Notify the app that the state is now anonymous
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
            return Task.CompletedTask;
        }
    }
}