using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface ITransactionLogic
    {
        Task DeleteTransactionAsync(Guid Id, string userId);
        Task UpdateTransactionAsync(Guid Id,TransactionUpdateDto transactiondto, string userId);
        Task CreateTransactionAsync(TransactionCreateDto transactiondto, string userId);
        Task<TransactionViewDto?> GetTransactionByIdAsync(Guid Id, string userId);
        Task<IEnumerable<TransactionViewDto?>> GetTransactionsByUserIdFilteredAsync(string userId, DateTime startDate, DateTime endDate);
        Task CreateBulkTransactionsAsync(BulkTransactionCreateDto bulkDto, string userId);
    }
}
