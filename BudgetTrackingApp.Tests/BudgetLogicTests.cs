using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Budget;
using Moq;
using Xunit;

namespace BudgetTrackingApp.Tests
{
    public class BudgetLogicTests
    {
        private readonly Mock<IBudgetRepository> _mockBudgetRepo;
        private readonly BudgetLogic _budgetLogic;

        public BudgetLogicTests()
        {
            _mockBudgetRepo = new Mock<IBudgetRepository>();
            _budgetLogic = new BudgetLogic(_mockBudgetRepo.Object);
        }

        [Fact]
        public async Task GetBudgetByUserIdAsync_ShouldReturnDto_WhenBudgetExists()
        {
            // ARRANGE
            var userId = "test-user";
            var budgetEntity = new Budget { LimitAmount = 50000, SpentAmount = 10000, AppUserId = userId };
            _mockBudgetRepo.Setup(r => r.GetBudgetByUserIdAsync(userId)).ReturnsAsync(budgetEntity);

            // ACT
            var result = await _budgetLogic.GetBudgetByUserIdAsync(userId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(50000, result.LimitAmount);
            Assert.Equal(10000, result.SpentAmount);
        }

        [Fact]
        public async Task UpdateBudgetLimitAsync_ShouldUpdateEntity_WhenBudgetExists()
        {
            // ARRANGE
            var userId = "test-user";
            var budgetEntity = new Budget { LimitAmount = 50000, AppUserId = userId };
            _mockBudgetRepo.Setup(r => r.GetBudgetByUserIdAsync(userId)).ReturnsAsync(budgetEntity);

            var updateDto = new BudgetUpdateDto { LimitAmount = 75000 };

            // ACT
            await _budgetLogic.UpdateBudgetLimitAsync(updateDto, userId);

            // ASSERT
            Assert.Equal(75000, budgetEntity.LimitAmount);
            _mockBudgetRepo.Verify(r => r.UpdateBudgetAsync(budgetEntity), Times.Once);
        }

        [Fact]
        public async Task UpdateBudgetLimitAsync_ShouldThrowException_WhenBudgetNotFound()
        {
            // ARRANGE
            var userId = "unknown-user";
            _mockBudgetRepo.Setup(r => r.GetBudgetByUserIdAsync(userId)).ReturnsAsync((Budget?)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(() =>
                _budgetLogic.UpdateBudgetLimitAsync(new BudgetUpdateDto(), userId));
        }
    }
}