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
            _mockHttp = new MockHttpMessageHandler();
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            Services.AddScoped(sp => httpClient);

            // Radzen Services
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
                    CategoryName = "Élelmiszer",
                    TransactionDate = DateTime.Now,
                    Type = TransactionType.Expense
                },
                new TransactionViewDto
                {
                    Id = Guid.NewGuid(),
                    Amount = 150000,
                    CategoryName = "Fizetés",
                    TransactionDate = DateTime.Now,
                    Type = TransactionType.Income
                }
            };

            // Bármilyen dátum paraméterrel hívják, ezt adja vissza
            _mockHttp.When(HttpMethod.Get, "http://localhost/api/transaction")
                     .RespondJson(transactions);

            // ACT
            var cut = Render<Transactions>();

            // Várunk, amíg a táblázat betölt (eltűnik a loading vagy megjelenik a sor)
            // A RadzenDataGrid trükkös, néha kell neki egy kis idő
            cut.WaitForState(() => cut.FindAll(".rz-datatable-data tr").Count > 0, TimeSpan.FromSeconds(2));

            // ASSERT
            // Ellenőrizzük, hogy a HTML tartalmazza-e a "Fizetés" szót
            Assert.Contains("Fizetés", cut.Markup);
            Assert.Contains("150 000", cut.Markup.Replace(" ", "\u00A0")); // Radzen formázás miatt (non-breaking space)
        }
    }
}