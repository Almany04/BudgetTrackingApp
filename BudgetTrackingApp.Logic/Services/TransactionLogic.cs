using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class TransactionLogic : ITransactionLogic
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;

        private const string DefaultSavingsCategoryName = "Megtakarítás";

        public TransactionLogic(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IBudgetRepository budgetRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
        }

        public async Task CreateTransactionAsync(TransactionCreateDto transactiondto, string userId)
        {
            // 1. Determine Category (Auto-assign for Savings if missing)
            Guid finalCategoryId = await ResolveCategoryIdAsync(transactiondto.CategoryId, transactiondto.Type, userId);

            // 2. Create Entity
            var newTransaction = new Transactions
            {
                Amount = transactiondto.Amount,
                TransactionDate = transactiondto.TransactionDate,
                Type = transactiondto.Type,
                Description = transactiondto.Description,
                CategoryId = finalCategoryId, // Resolved ID
                AppUserId = userId,
                Merchant = transactiondto.Merchant,
                PaymentMethod = transactiondto.PaymentMethod,
                PaidBy = transactiondto.PaidBy,
                IsSplit = transactiondto.IsSplit,
                MyShareRatio = transactiondto.MyShareRatio,
                SavingGoalId = transactiondto.SavingGoalId
            };

            await _transactionRepository.AddTransactionAsync(newTransaction);

            // 3. Update Budget (Only for Expenses)
            if (newTransaction.Type == TransactionType.Expense)
            {
                await UpdateBudgetSpentAmountAsync(userId, newTransaction.Amount, newTransaction.IsSplit, newTransaction.PaidBy, newTransaction.MyShareRatio, isAdding: true);
            }
        }

        public async Task UpdateTransactionAsync(Guid Id, TransactionUpdateDto transactiondto, string userId)
        {
            var isOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!isOwned) throw new Exception("Access denied!");

            var tx = await _transactionRepository.GetTransactionByIdAsync(Id);
            if (tx == null) throw new Exception("Transaction not found.");

            // 1. Revert Budget Impact of OLD transaction
            if (tx.Type == TransactionType.Expense)
            {
                await UpdateBudgetSpentAmountAsync(userId, tx.Amount, tx.IsSplit, tx.PaidBy, tx.MyShareRatio, isAdding: false);
            }

            // 2. Resolve Category (Auto-assign for Savings if missing)
            Guid finalCategoryId = await ResolveCategoryIdAsync(transactiondto.CategoryId, transactiondto.Type, userId);

            // 3. Update Entity
            tx.Amount = transactiondto.Amount;
            tx.TransactionDate = transactiondto.TransactionDate;
            tx.Description = transactiondto.Description;
            tx.Type = transactiondto.Type;
            tx.CategoryId = finalCategoryId; // Resolved ID
            tx.Merchant = transactiondto.Merchant;
            tx.PaymentMethod = transactiondto.PaymentMethod;
            tx.PaidBy = transactiondto.PaidBy;
            tx.IsSplit = transactiondto.IsSplit;
            tx.MyShareRatio = transactiondto.MyShareRatio;
            tx.SavingGoalId = transactiondto.SavingGoalId;

            await _transactionRepository.UpdateTransactionAsync(tx);

            // 4. Apply Budget Impact of NEW transaction
            if (transactiondto.Type == TransactionType.Expense)
            {
                await UpdateBudgetSpentAmountAsync(userId, transactiondto.Amount, transactiondto.IsSplit, transactiondto.PaidBy, transactiondto.MyShareRatio, isAdding: true);
            }
        }

        public async Task DeleteTransactionAsync(Guid Id, string userId)
        {
            var isOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!isOwned) return;

            var tx = await _transactionRepository.GetTransactionByIdAsync(Id);
            if (tx == null) return;

            if (tx.Type == TransactionType.Expense)
            {
                await UpdateBudgetSpentAmountAsync(userId, tx.Amount, tx.IsSplit, tx.PaidBy, tx.MyShareRatio, isAdding: false);
            }
            await _transactionRepository.DeleteTransactionAsync(tx);
        }

        // --- Helper Methods for Clean Code ---

        private async Task<Guid> ResolveCategoryIdAsync(Guid? categoryId, TransactionType type, string userId)
        {
            if (type == TransactionType.Saving)
            {
                // If user provided a category, verify it
                if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                {
                    var valid = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId.Value, userId);
                    if (!valid) throw new Exception("Access denied to this category.");
                    return categoryId.Value;
                }

                // If NO category provided, Find or Create "Megtakarítás"
                var userCategories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);
                var savingsCat = userCategories.FirstOrDefault(c => c.Name.Equals(DefaultSavingsCategoryName, StringComparison.OrdinalIgnoreCase));

                if (savingsCat == null)
                {
                    savingsCat = new Category { Name = DefaultSavingsCategoryName, AppUserId = userId };
                    await _categoryRepository.AddCategoryAsync(savingsCat);
                }
                return savingsCat.Id;
            }
            else
            {
                // For Expense/Income, Category is strictly required
                if (!categoryId.HasValue || categoryId.Value == Guid.Empty)
                    throw new Exception("Category selection is required for this transaction type.");

                var valid = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId.Value, userId);
                if (!valid) throw new Exception("Access denied to this category.");

                return categoryId.Value;
            }
        }

        private async Task UpdateBudgetSpentAmountAsync(string userId, decimal amount, bool isSplit, PaidBy paidBy, decimal shareRatio, bool isAdding)
        {
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);
            if (budget == null) return;

            decimal myCost = isSplit
                ? amount * shareRatio
                : (paidBy == PaidBy.Me ? amount : 0);

            if (isAdding)
                budget.SpentAmount += myCost;
            else
                budget.SpentAmount -= myCost;

            await _budgetRepository.UpdateBudgetAsync(budget);
        }

        // --- Existing Read Methods ---

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

            decimal totalMyExpenseDelta = 0;
            var receiptId = Guid.NewGuid();

            foreach (var item in bulkDto.Items)
            {
                // For bulk items, we assume they are Expenses, so we check category strictly
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
                    PaidBy = bulkDto.PaidBy,
                    IsSplit = bulkDto.IsSplit,
                    MyShareRatio = bulkDto.MyShareRatio,
                    SavingGoalId = null
                };

                if (item.Type == TransactionType.Expense)
                {
                    decimal itemCost = tx.IsSplit
                        ? tx.Amount * tx.MyShareRatio
                        : (tx.PaidBy == PaidBy.Me ? tx.Amount : 0);
                    totalMyExpenseDelta += itemCost;
                }

                await _transactionRepository.AddTransactionAsync(tx);
            }

            if (totalMyExpenseDelta > 0)
            {
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budget != null)
                {
                    budget.SpentAmount += totalMyExpenseDelta;
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
                MyShareRatio = entity.MyShareRatio,
                SavingGoalId = entity.SavingGoalId,
                SavingGoalName = entity.SavingGoal?.Name
            };
        }
    }
}