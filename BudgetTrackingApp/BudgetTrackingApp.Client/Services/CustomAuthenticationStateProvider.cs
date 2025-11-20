using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BudgetTrackingApp.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private const string UserSessionKey = "userSession";

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // CRITICAL FIX: Always validate with the server first!
                // This ensures Client and Server are always in sync.
                UserLoginResponseDto? userDto = null;

                try
                {
                    // Ask server: "Who am I?"
                    userDto = await _httpClient.GetFromJsonAsync<UserLoginResponseDto>("api/user/current");
                }
                catch
                {
                    // If server says 401 or fails, we are definitely logged out.
                    userDto = null;
                }

                if (userDto != null && !string.IsNullOrEmpty(userDto.UserId))
                {
                    // Server says we are logged in. Update session storage.
                    await SaveSessionAsync(userDto);
                    return CreateAuthenticationState(userDto);
                }
                else
                {
                    // Server says we are logged out. Clear any stale client data.
                    await ClearSessionAsync();
                    return new AuthenticationState(_anonymous);
                }
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        private AuthenticationState CreateAuthenticationState(UserLoginResponseDto userDto)
        {
            var claims = new[]
           {
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                new Claim(ClaimTypes.Email, userDto.Email)
            };
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth")));
        }

        public async Task LoginAsync(UserLoginResponseDto userDto)
        {
            await SaveSessionAsync(userDto);
            NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(userDto)));
        }

        public async Task LogoutAsync()
        {
            await ClearSessionAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        private async Task SaveSessionAsync(UserLoginResponseDto userDto)
        {
            var userSessionJson = JsonSerializer.Serialize(userDto);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", UserSessionKey, userSessionJson);
        }

        private async Task ClearSessionAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", UserSessionKey);
        }
    }
}