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

        // JAVÍTÁS: Átnevezve, és a tároló is változik
        private const string UserSessionKey = "userSession";

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // JAVÍTÁS: localStorage -> sessionStorage
                var userSessionJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", UserSessionKey);

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
            // JAVÍTÁS: localStorage -> sessionStorage
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", UserSessionKey, userSessionJson);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                new Claim(ClaimTypes.Email, userDto.Email)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task LogoutAsync()
        {
            // JAVÍTÁS: localStorage -> sessionStorage
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", UserSessionKey);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}