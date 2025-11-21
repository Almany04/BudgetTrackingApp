using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using Moq; 
using Xunit; 

namespace BudgetTrackingApp.Tests
{
    public class TransactionLogicTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IBudgetRepository> _mockBudgetRepo;
        private readonly TransactionLogic _transactionLogic;

        public TransactionLogicTests()
        {
            // 1. Elõkészítjük a "hamis" (Mock) repository-kat
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockBudgetRepo = new Mock<IBudgetRepository>();

            // 2. Létrehozzuk a tesztelendõ logikát a hamis függõségekkel
            _transactionLogic = new TransactionLogic(
                _mockTransactionRepo.Object,
                _mockCategoryRepo.Object,
                _mockBudgetRepo.Object
            );
        }

        [Fact]
        public async Task CreateTransaction_ShouldDecreaseBudget_WhenTypeIsExpense()
        {
            // ARRANGE (Elõkészítés)
            var userId = "test-user-id";
            var categoryId = Guid.NewGuid();

            // Szimuláljuk, hogy a kategória létezik és a felhasználóé
            _mockCategoryRepo.Setup(repo => repo.IsCategoryOwnedByUserAsync(categoryId, userId))
                             .ReturnsAsync(true);

            // Szimulálunk egy meglévõ büdzsét (pl. 100.000 Ft limit, 20.000 Ft eddigi költés)
            var initialBudget = new Budget { LimitAmount = 100000, SpentAmount = 20000, AppUserId = userId };
            _mockBudgetRepo.Setup(repo => repo.GetBudgetByUserIdAsync(userId))
                           .ReturnsAsync(initialBudget);

            // Ez a tranzakció, amit hozzá akarunk adni (pl. 5.000 Ft kiadás)
            var transactionDto = new TransactionCreateDto
            {
                Amount = 5000,
                Type = TransactionType.Expense,
                CategoryId = categoryId,
                TransactionDate = DateTime.Now,
                Description = "Teszt vásárlás"
            };

            // ACT (Végrehajtás)
            await _transactionLogic.CreateTransactionAsync(transactionDto, userId);

            // ASSERT (Ellenõrzés)
            // 1. Ellenõrizzük, hogy a büdzsé "SpentAmount" értéke nõtt-e 5000-rel (20.000 -> 25.000)
            Assert.Equal(25000, initialBudget.SpentAmount);

            // 2. Ellenõrizzük, hogy a rendszer meghívta-e a BudgetRepository.UpdateBudgetAsync metódust
            _mockBudgetRepo.Verify(repo => repo.UpdateBudgetAsync(initialBudget), Times.Once);

            // 3. Ellenõrizzük, hogy a rendszer meghívta-e a TransactionRepository.AddTransactionAsync metódust
            _mockTransactionRepo.Verify(repo => repo.AddTransactionAsync(It.IsAny<Transactions>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_ShouldThrowException_WhenCategoryNotOwned()
        {
            // ARRANGE - Szimuláljuk, hogy a kategória NEM a felhasználóé
            var userId = "test-user-id";
            var categoryId = Guid.NewGuid();
            _mockCategoryRepo.Setup(repo => repo.IsCategoryOwnedByUserAsync(categoryId, userId))
                             .ReturnsAsync(false);

            var transactionDto = new TransactionCreateDto { CategoryId = categoryId };

            // ACT & ASSERT - Kivételt várunk
            await Assert.ThrowsAsync<Exception>(() => _transactionLogic.CreateTransactionAsync(transactionDto, userId));
        }
    }
}