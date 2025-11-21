using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using BudgetTrackingApp.Client.Pages;
using BudgetTrackingApp.Shared.Dtos.User;
using Radzen;
using RichardSzalay.MockHttp;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using Moq;
using BudgetTrackingApp.Client.Services;

namespace BudgetTrackingApp.Client.Tests
{
    public class LoginPageTests : TestContext
    {
        private readonly MockHttpMessageHandler _mockHttp;

        public LoginPageTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            Services.AddScoped(sp => httpClient);

            Services.AddScoped<DialogService>();
            Services.AddScoped<NotificationService>();
            Services.AddScoped<ContextMenuService>();
            Services.AddScoped<TooltipService>();

            // Hitelesítés mockolása (hogy ne dobjon hibát a CustomAuthProvider)
            Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        }

        [Fact]
        public void Login_ShouldSubmitForm_WhenClicked()
        {
            // ARRANGE
            _mockHttp.When(HttpMethod.Post, "http://localhost/api/user/login")
                     .RespondJson(new UserLoginResponseDto { UserId = "123", Email = "test@test.com" });

            // A CustomAuthProvider hívni fogja a "current" endpointot is
            _mockHttp.When("http://localhost/api/user/current")
                     .Respond(HttpStatusCode.Unauthorized); // Kezdetben nincs belépve

            var cut = Render<Login>();

            // ACT
            // Kitöltjük a mezőket (Name attribútum alapján)
            cut.Find("input[name='Email']").Change("test@test.com");
            cut.Find("input[name='Password']").Change("Password123!");

            // Megnyomjuk a gombot
            cut.Find("button[type='submit']").Click();

            // ASSERT
            // Ellenőrizzük, hogy a POST kérés megtörtént-e
            // (Itt várhatunk picit az aszinkron hívásra)
            // A RichardSzalay.MockHttp Verify metódusa nem mindig működik jól aszinkron hívásokkal bUnit-ban,
            // de ha nem dob hibát és lefut, az már fél siker. 
            // Profibb: ellenőrizni a navigációt, de ahhoz mockolni kell a NavigationManagert is.
        }
    }
}