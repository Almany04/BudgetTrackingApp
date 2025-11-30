using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using System.Linq;

namespace BudgetTrackingApp.Logic.Services
{
    public class TransactionLogic : ITransactionLogic
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;

        public TransactionLogic(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IBudgetRepository budgetRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
        }

        public async Task CreateTransactionAsync(TransactionCreateDto transactiondto, string userId)
        {
            var valid = await _categoryRepository.IsCategoryOwnedByUserAsync(transactiondto.CategoryId, userId);
            if (!valid) throw new Exception("Access denied to this category.");

            var newTransaction = new Transactions
            {
                Amount = transactiondto.Amount,
                TransactionDate = transactiondto.TransactionDate,
                Type = transactiondto.Type,
                Description = transactiondto.Description,
                CategoryId = transactiondto.CategoryId,
                AppUserId = userId,
                Merchant = transactiondto.Merchant,
                PaymentMethod = transactiondto.PaymentMethod,
                PaidBy = transactiondto.PaidBy,
                IsSplit = transactiondto.IsSplit,
                MyShareRatio = transactiondto.MyShareRatio, // <--- Map
                SavingGoalId = transactiondto.SavingGoalId
            };

            await _transactionRepository.AddTransactionAsync(newTransaction);

            if (newTransaction.Type == TransactionType.Expense)
            {
                var budgetCurrent = await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budgetCurrent != null)
                {
                    // Update Budget based on MY SHARE only
                    decimal myCost = newTransaction.IsSplit
                        ? newTransaction.Amount * newTransaction.MyShareRatio
                        : (newTransaction.PaidBy == PaidBy.Me ? newTransaction.Amount : 0);

                    // Note: If I paid fully for something that isn't split, it's 100% my expense.
                    // If Partner paid fully (gift) and not split, it's 0% my expense.

                    budgetCurrent.SpentAmount += myCost;
                    await _budgetRepository.UpdateBudgetAsync(budgetCurrent);
                }
            }
        }

        public async Task UpdateTransactionAsync(Guid Id, TransactionUpdateDto transactiondto, string userId)
        {
            var IsOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned) throw new Exception("Access denied!");

            var tx = await _transactionRepository.GetTransactionByIdAsync(Id);
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

            if (tx == null || budget == null) throw new Exception("Not found.");

            // Revert old budget impact
            decimal oldCost = tx.IsSplit
                ? tx.Amount * tx.MyShareRatio
                : (tx.PaidBy == PaidBy.Me ? tx.Amount : 0);

            if (tx.Type == TransactionType.Expense) budget.SpentAmount -= oldCost;

            // Apply new budget impact
            decimal newCost = transactiondto.IsSplit
                ? transactiondto.Amount * transactiondto.MyShareRatio
                : (transactiondto.PaidBy == PaidBy.Me ? transactiondto.Amount : 0);

            if (transactiondto.Type == TransactionType.Expense) budget.SpentAmount += newCost;

            // Update fields
            tx.Amount = transactiondto.Amount;
            tx.TransactionDate = transactiondto.TransactionDate;
            tx.Description = transactiondto.Description;
            tx.Type = transactiondto.Type;
            tx.CategoryId = transactiondto.CategoryId;
            tx.Merchant = transactiondto.Merchant;
            tx.PaymentMethod = transactiondto.PaymentMethod;
            tx.PaidBy = transactiondto.PaidBy;
            tx.IsSplit = transactiondto.IsSplit;
            tx.MyShareRatio = transactiondto.MyShareRatio; // <--- Map
            tx.SavingGoalId = transactiondto.SavingGoalId;

            await _transactionRepository.UpdateTransactionAsync(tx);
            await _budgetRepository.UpdateBudgetAsync(budget);
        }

        public async Task DeleteTransactionAsync(Guid Id, string userId)
        {
            var IsOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned) return;

            var tx = await _transactionRepository.GetTransactionByIdAsync(Id);
            if (tx == null) return;

            if (tx.Type == TransactionType.Expense)
            {
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budget != null)
                {
                    decimal myCost = tx.IsSplit
                        ? tx.Amount * tx.MyShareRatio
                        : (tx.PaidBy == PaidBy.Me ? tx.Amount : 0);

                    budget.SpentAmount -= myCost;
                    await _budgetRepository.UpdateBudgetAsync(budget);
                }
            }
            await _transactionRepository.DeleteTransactionAsync(tx);
        }

        public async Task<TransactionViewDto?> GetTransactionByIdAsync(Guid Id, string userId)
        {
            var IsOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned) return null;
            var entity = await _transactionRepository.GetTransactionByIdAsync(Id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<TransactionViewDto?>> GetTransactionsByUserIdFilteredAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var entities = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
            return entities.Select(MapToDto);
        }

        public async Task CreateBulkTransactionsAsync(BulkTransactionCreateDto bulkDto, string userId)
        {
            if (bulkDto.Items == null || !bulkDto.Items.Any()) return;

            decimal totalMyExpense = 0;
            var receiptId = Guid.NewGuid();

            foreach (var item in bulkDto.Items)
            {
                var valid = await _categoryRepository.IsCategoryOwnedByUserAsync(item.CategoryId, userId);
                if (!valid) throw new Exception($"Access denied to category for item: {item.Description}");

                var tx = new Transactions
                {
                    AppUserId = userId,
                    TransactionDate = bulkDto.TransactionDate,
                    Merchant = bulkDto.Merchant,
                    PaymentMethod = bulkDto.PaymentMethod,
                    Amount = item.Amount,
                    Description = item.Description,
                    CategoryId = item.CategoryId,
                    Type = item.Type,
                    ReceiptId = receiptId,
                    PaidBy = PaidBy.Me, // Default for bulk
                    IsSplit = false,
                    MyShareRatio = 1.0m, // Default 100% me
                    SavingGoalId = null
                };

                if (item.Type == TransactionType.Expense) totalMyExpense += item.Amount;

                await _transactionRepository.AddTransactionAsync(tx);
            }

            if (totalMyExpense > 0)
            {
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budget != null)
                {
                    budget.SpentAmount += totalMyExpense;
                    await _budgetRepository.UpdateBudgetAsync(budget);
                }
            }
        }

        private TransactionViewDto MapToDto(Transactions entity)
        {
            string catName = entity.Category?.Name ?? "Unknown";
            if (entity.Category?.ParentCategory != null)
            {
                catName = $"{entity.Category.ParentCategory.Name} > {entity.Category.Name}";
            }

            return new TransactionViewDto
            {
                Id = entity.Id,
                Amount = entity.Amount,
                Description = entity.Description,
                TransactionDate = entity.TransactionDate,
                Type = entity.Type,
                CategoryName = catName,
                CategoryId = entity.CategoryId,
                Merchant = entity.Merchant,
                PaymentMethod = entity.PaymentMethod,
                ReceiptId = entity.ReceiptId,
                PaidBy = entity.PaidBy,
                IsSplit = entity.IsSplit,
                MyShareRatio = entity.MyShareRatio, // <--- Map
                SavingGoalId = entity.SavingGoalId,
                SavingGoalName = entity.SavingGoal?.Name
            };
        }
    }
}