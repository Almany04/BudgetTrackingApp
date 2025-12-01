using BudgetTrackingApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Repository.Interfaces
{
    public interface ISavingGoalRepository
    {
        Task<List<SavingGoal>> GetGoalsAsync(Guid userId);
        Task<SavingGoal?> GetGoalByIdAsync(Guid id, Guid userId);
        Task AddGoalAsync(SavingGoal goal);
        Task UpdateGoalAsync(SavingGoal goal);
        Task DeleteGoalAsync(SavingGoal goal);
    }
}
