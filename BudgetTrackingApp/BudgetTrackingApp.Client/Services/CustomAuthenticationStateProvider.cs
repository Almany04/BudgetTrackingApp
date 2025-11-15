using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;

namespace BudgetTrackingApp.Client.Services
{
  
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        
        private const string UserSessionStorageKey = "userSession";

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserSessionStorageKey);

                if (string.IsNullOrWhiteSpace(userSessionJson))
                {
                    return new AuthenticationState(_anonymous);
                }

                var userDto = JsonSerializer.Deserialize<UserLoginResponseDto>(userSessionJson);

                if (userDto == null)
                {
                    return new AuthenticationState(_anonymous);
                }

               
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                    new Claim(ClaimTypes.Email, userDto.Email)
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task LoginAsync(UserLoginResponseDto userDto)
        {
            var userSessionJson = JsonSerializer.Serialize(userDto);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserSessionStorageKey, userSessionJson);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                new Claim(ClaimTypes.Email, userDto.Email)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var user = new ClaimsPrincipal(identity);

            // Értesítjük a Blazor-t, hogy a felhasználó állapota megváltozott (bejelentkezett)
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserSessionStorageKey);

            // Értesítjük a Blazor-t, hogy a felhasználó kijelentkezett
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}