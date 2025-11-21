using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using BudgetTrackingApp.Client.Pages;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using Radzen;
using RichardSzalay.MockHttp;
using System.Net;

namespace BudgetTrackingApp.Client.Tests
{
    public class TransactionsPageTests : TestContext
    {
        private readonly MockHttpMessageHandler _mockHttp;

        public TransactionsPageTests()
        {
            // --- EZ A KULCS A JAVÍTÁSHOZ ---
            // Engedélyezzük a váratlan JS hívásokat (pl. Radzen.preventArrows)
            JSInterop.Mode = JSRuntimeMode.Loose;
            // -------------------------------

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
        
        public void TransactionsPage_ShouldLoadAndRenderTransactions()
        {
            // ARRANGE
            var transactions = new List<TransactionViewDto>
            {
                new TransactionViewDto
                {
                    Id = Guid.NewGuid(),
                    Amount = 5000,
                    CategoryName = "Elelmiszer", // Ékezet nélkül a biztonság kedvéért
                    TransactionDate = DateTime.Now,
                    Type = TransactionType.Expense
                },
                new TransactionViewDto
                {
                    Id = Guid.NewGuid(),
                    Amount = 150000,
                    CategoryName = "Fizetes",
                    TransactionDate = DateTime.Now,
                    Type = TransactionType.Income
                }
            };

            _mockHttp.When(HttpMethod.Get, "http://localhost/api/transaction")
                     .RespondJson(transactions);

            // ACT
            var cut = Render<Transactions>();

            // JAVÍTÁS: Nem a DataGrid belső soraira várunk, hanem magára a szövegre a HTML-ben.
            // Ez sokkal robusztusabb bUnit-ban.
            cut.WaitForState(() => cut.Markup.Contains("Fizetes"), TimeSpan.FromSeconds(3));

            // ASSERT
            Assert.Contains("Fizetes", cut.Markup);
            Assert.Contains("150", cut.Markup);
        }
    }
}