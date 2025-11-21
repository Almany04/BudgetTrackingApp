using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Net.Http.Json;

namespace BudgetTrackingApp.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
       
        private readonly AuthenticationState _anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {

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
                
            }

            return _anonymousState;
        }

   
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

        
        public Task LogoutAsync()
        {
   
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
            return Task.CompletedTask;
        }
    }
}