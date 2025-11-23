using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Repository.Implamentations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly BudgetTrackerDbContext _context;

        public TransactionRepository(BudgetTrackerDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(Transactions transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(Transactions transaction)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<Transactions?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _context.Transactions
                                 .Include(t=>t.Category)
                                 .ThenInclude(c => c.ParentCategory)
                                 .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        public async Task<IEnumerable<Transactions>> GetTransactionsByUserIdAsync(string userId)
        {
            var transactions= await _context.Transactions
                                    .Include(t => t.Category)
                                    .ThenInclude(c => c.ParentCategory)
                                    .Where(t=>t.AppUserId==userId)
                                    .ToListAsync();
            return transactions;
        }

        public async Task<IEnumerable<Transactions>> GetTransactionsByUserIdFilteredAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transactions= await _context.Transactions.
                                      Include(t=>t.Category)
                                      .ThenInclude(c => c.ParentCategory)
                                      .Where(t=>t.AppUserId==userId)
                                      .Where(t=>t.TransactionDate>=startDate&&t.TransactionDate<=endDate)
                                      .OrderByDescending(t=>t.TransactionDate)
                                      .ToListAsync();
            return transactions;
        }

        public async Task<bool> HasTransactionsForCategoryAsync(Guid categoryId)
        {
            return await _context.Transactions.AnyAsync(t => t.CategoryId == categoryId);
        }

        public async Task<bool> IsTransactionOwnedByIdAsync(Guid transactionId, string userId)
        {
            return await _context.Transactions.AnyAsync(t=>t.Id==transactionId&&t.AppUserId==userId);
        }

        
        public async Task UpdateTransactionAsync(Transactions transaction)
        {
           _context.Transactions.Update(transaction);

           await _context.SaveChangesAsync();
        }
    }
}
