using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using System.Net.Http.Json;
using Xunit;

namespace BudgetTrackingApp.IntegrationTests
{
    public class TransactionControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public TransactionControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
           
            _client = factory.CreateClient();
        }

        

        [Fact]
        public async Task CreateTransaction_ReturnsCreated_AndSavesToDatabase()
        {
          
        }
    }
}