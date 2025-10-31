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
    public class BudgetRepository : IBudgetRepository
    {
        private readonly BudgetTrackerDbContext _context;

        public BudgetRepository(BudgetTrackerDbContext context)
        {
            _context = context;
        }
        public async Task<Budget?> GetBudgetByUserIdAsync(string userId)
        {
            return await _context.Budgets.FirstOrDefaultAsync(b => b.AppUserId == userId);
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            _context.Budgets.Update(budget);

            await _context.SaveChangesAsync();
        }
    }
}
