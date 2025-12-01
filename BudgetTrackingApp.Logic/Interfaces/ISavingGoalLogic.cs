using BudgetTrackingApp.Shared.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface ISavingGoalLogic
    {
        Task<List<SavingGoalDto>> GetGoalsAsync(Guid userId);
        Task CreateGoalAsync(Guid userId, SavingGoalDto dto);
        Task<bool> UpdateGoalAsync(Guid userId, Guid goalId, SavingGoalUpdateDto dto);
        Task<bool> DeleteGoalAsync(Guid userId, Guid goalId);
    }
}
