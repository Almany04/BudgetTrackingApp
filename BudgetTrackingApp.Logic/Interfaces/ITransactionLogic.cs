using BudgetTrackingApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface ITransactionLogic
    {
        Task<Transactions?> GetTransactionByIdAsync(Guid transactionId);
        Task DeleteTransactionAsync(Transactions transaction);
        Task UpdateTransactionAsync(Transactions transaction);
        Task AddTransactionAsync(Transactions transaction);

        Task<IEnumerable<Transactions>> GetTransactionsByUserIdAsync(string userId);

        Task<bool> IsTransactionOwnedByIdAsync(Guid transactionId, string userId);
        Task<bool> HasTransactionsForCategoryAsync(Guid categoryId);
        Task<IEnumerable<Transactions>> GetTransactionsByUserIdFilteredAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
