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
    public class SavingGoalRepository : ISavingGoalRepository
    {
        private readonly BudgetTrackerDbContext _context;

        public SavingGoalRepository(BudgetTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<List<SavingGoal>> GetGoalsAsync(Guid userId)
        {
            return await _context.SavingGoals
                .Where(g => g.AppUserId == userId.ToString())
                .ToListAsync();
        }

        public async Task<SavingGoal?> GetGoalByIdAsync(Guid id, Guid userId)
        {
            return await _context.SavingGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId.ToString());
        }

        public async Task AddGoalAsync(SavingGoal goal)
        {
            await _context.SavingGoals.AddAsync(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGoalAsync(SavingGoal goal)
        {
            _context.SavingGoals.Update(goal);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(SavingGoal goal)
        {
            _context.SavingGoals.Remove(goal);
            await _context.SaveChangesAsync();
        }
    }
}
