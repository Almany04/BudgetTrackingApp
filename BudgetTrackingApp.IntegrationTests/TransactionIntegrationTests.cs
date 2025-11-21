using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;

namespace BudgetTrackingApp.IntegrationTests
{
    public class TransactionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public TransactionIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            // Beállítjuk, hogy a kérések automatikusan a "TestScheme" hitelesítést használják
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task CreateAndGetTransactions_FullFlow_ShouldWork()
        {
            // 1. ELŐKÉSZÍTÉS (Adatbázis feltöltése kategóriával)
            // Mivel az adatbázis üres, először kell egy kategória, amihez a tranzakciót kötjük
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BudgetTrackerDbContext>();
                var category = new Category { Id = Guid.NewGuid(), Name = "Teszt Kat", AppUserId = "test-user-id" };
                var budget = new Budget { Id = Guid.NewGuid(), AppUserId = "test-user-id", LimitAmount = 100000, SpentAmount = 0 };

                db.Categories.Add(category);
                db.Budgets.Add(budget);
                await db.SaveChangesAsync();

                // 2. LÉTREHOZÁS (POST hívás a Controllerre)
                var newTransaction = new TransactionCreateDto
                {
                    Amount = 5000,
                    Type = TransactionType.Expense,
                    CategoryId = category.Id,
                    TransactionDate = DateTime.Now,
                    Description = "Integration Test Expense"
                };

                var postResponse = await _client.PostAsJsonAsync("/api/transaction", newTransaction);

                // Ellenőrizzük, hogy sikeres volt-e (201 Created)
                postResponse.EnsureSuccessStatusCode();
            }

            // 3. LEKÉRDEZÉS (GET hívás)
            // Megnézzük, hogy a lista visszaadja-e az imént létrehozott elemet
            var getResponse = await _client.GetAsync("/api/transaction");
            getResponse.EnsureSuccessStatusCode();

            var transactions = await getResponse.Content.ReadFromJsonAsync<List<TransactionViewDto>>();

            // 4. ELLENŐRZÉS
            Assert.NotNull(transactions);
            Assert.NotEmpty(transactions);
            Assert.Contains(transactions, t => t.Description == "Integration Test Expense" && t.Amount == 5000);
        }
    }
}