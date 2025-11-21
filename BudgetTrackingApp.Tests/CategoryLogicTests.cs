using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using Moq;
using Xunit;

namespace BudgetTrackingApp.Tests
{
    public class CategoryLogicTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly CategoryLogic _categoryLogic;

        public CategoryLogicTests()
        {
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _categoryLogic = new CategoryLogic(_mockCategoryRepo.Object, _mockTransactionRepo.Object);
        }

        [Fact]
        public async Task CreateCategory_ShouldThrowException_WhenNameIsDuplicate()
        {
            // ARRANGE
            var userId = "user1";
            var duplicateName = "Élelmiszer";
            var existingCategories = new List<Category>
            {
                new Category { Name = "Élelmiszer", AppUserId = userId }
            };

            // Azt hazudjuk (Mock), hogy már van ilyen kategória az adatbázisban
            _mockCategoryRepo.Setup(repo => repo.GetCategoriesByUserIdAsync(userId))
                             .ReturnsAsync(existingCategories);

            var newCategoryDto = new CategoryCreateDto { Name = "Élelmiszer" };

            // ACT & ASSERT
            // Ellenőrizzük, hogy a rendszer dob-e hibát duplikáció esetén
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _categoryLogic.CreateCategoryAsync(newCategoryDto, userId));

            Assert.Equal($"Már létezik'{duplicateName}' nevű kategória!", exception.Message);
        }

        [Fact]
        public async Task DeleteCategory_ShouldThrowException_WhenCategoryHasTransactions()
        {
            // ARRANGE
            var userId = "user1";
            var categoryId = Guid.NewGuid();

            // A kategória a felhasználóé...
            _mockCategoryRepo.Setup(repo => repo.IsCategoryOwnedByUserAsync(categoryId, userId))
                             .ReturnsAsync(true);
            // ...DE van hozzá tartozó tranzakció!
            _mockTransactionRepo.Setup(repo => repo.HasTransactionsForCategoryAsync(categoryId))
                                .ReturnsAsync(true);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(() => _categoryLogic.DeleteCategoryAsync(categoryId, userId));
        }
    }
}