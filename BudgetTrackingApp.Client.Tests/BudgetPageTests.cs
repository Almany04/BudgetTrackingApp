using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using BudgetTrackingApp.Client.Pages;
using BudgetTrackingApp.Shared.Dtos.Budget;
using Radzen;
using RichardSzalay.MockHttp;
using System.Net;

namespace BudgetTrackingApp.Client.Tests
{
    public class BudgetPageTests : TestContext
    {
        private readonly MockHttpMessageHandler _mockHttp;

        public BudgetPageTests()
        {
            // JSInterop beállítása, hogy a Radzen hívások ne okozzanak gondot
            JSInterop.Mode = JSRuntimeMode.Loose;

            _mockHttp = new MockHttpMessageHandler();
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");

            Services.AddScoped(sp => httpClient);
            Services.AddScoped<DialogService>();
            Services.AddScoped<NotificationService>();
            Services.AddScoped<ContextMenuService>();
            Services.AddScoped<TooltipService>();
        }

        [Fact]
        public void BudgetPage_ShouldLoadAndDisplayData()
        {
            _mockHttp.When(HttpMethod.Get, "http://localhost/api/budget")
                     .RespondJson(new BudgetViewDto { LimitAmount = 150000, SpentAmount = 45000 });

            var cut = Render<Budget>();

            // Megvárjuk, amíg betölt (keressük a 'Save' gombot vagy az inputot)
            cut.WaitForState(() => cut.FindAll("input").Count > 0);

            // JAVÍTVA: Name attribútum alapján keressük az inputot
            var input = cut.Find("input[name='LimitAmount']");

            Assert.Contains("150000", input.GetAttribute("value"));
        }

        [Fact]
        public void BudgetPage_ShouldSendPutRequest_WhenSaveClicked()
        {
            _mockHttp.When(HttpMethod.Get, "http://localhost/api/budget")
                     .RespondJson(new BudgetViewDto { LimitAmount = 100000 });

            _mockHttp.When(HttpMethod.Put, "http://localhost/api/budget")
                     .WithJsonContent(new BudgetUpdateDto { LimitAmount = 250000 })
                     .Respond(HttpStatusCode.OK);

            var cut = Render<Budget>();
            cut.WaitForState(() => cut.FindAll("input").Count > 0);

            var input = cut.Find("input[name='LimitAmount']");
            input.Change("250000");

            var form = cut.Find("form");
            form.Submit();

            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}