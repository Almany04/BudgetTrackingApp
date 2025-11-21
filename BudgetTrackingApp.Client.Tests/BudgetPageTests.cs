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
            _mockHttp = new MockHttpMessageHandler();
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");

            Services.AddScoped(sp => httpClient);

            // Radzen szolgáltatások
            Services.AddScoped<DialogService>();
            Services.AddScoped<NotificationService>();
            Services.AddScoped<ContextMenuService>();
            Services.AddScoped<TooltipService>();
        }

        [Fact]
        public void BudgetPage_ShouldLoadAndDisplayData()
        {
            // --- ARRANGE ---
            _mockHttp.When(HttpMethod.Get, "http://localhost/api/budget")
                     .RespondJson(new BudgetViewDto
                     {
                         LimitAmount = 150000,
                         SpentAmount = 45000
                     });

            // --- ACT ---
            var cut = Render<Budget>();

            // Várunk, amíg megjelenik az input (azaz betöltõdött az adat)
            cut.WaitForState(() => cut.FindAll("input").Count > 0);

            // --- ASSERT ---
            // JAVÍTVA: Biztosabb szelektor a Name attribútum alapján
            var input = cut.Find("input[name='LimitAmount']");

            // Ellenõrizzük az értéket
            Assert.Contains("150000", input.GetAttribute("value"));
        }

        [Fact]
        public void BudgetPage_ShouldSendPutRequest_WhenSaveClicked()
        {
            // --- ARRANGE ---
            _mockHttp.When(HttpMethod.Get, "http://localhost/api/budget")
                     .RespondJson(new BudgetViewDto { LimitAmount = 100000 });

            var updateRequest = _mockHttp.When(HttpMethod.Put, "http://localhost/api/budget")
                                         .WithJsonContent(new BudgetUpdateDto { LimitAmount = 250000 })
                                         .Respond(HttpStatusCode.OK);

            var cut = Render<Budget>();

            cut.WaitForState(() => cut.FindAll("input").Count > 0);

            // --- ACT ---
            // JAVÍTVA: Itt is a Name alapú szelektor
            var input = cut.Find("input[name='LimitAmount']");
            input.Change("250000");

            // A form submit gomb megnyomása
            var form = cut.Find("form");
            form.Submit();

            // --- ASSERT ---
            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}