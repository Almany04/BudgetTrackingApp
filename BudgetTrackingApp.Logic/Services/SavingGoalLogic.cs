using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class SavingGoalLogic : ISavingGoalLogic
    {
        private readonly ISavingGoalRepository _repository;

        public SavingGoalLogic(ISavingGoalRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SavingGoalDto>> GetGoalsAsync(Guid userId)
        {
            var goals = await _repository.GetGoalsAsync(userId);

            return goals.Select(g => new SavingGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount
            }).ToList();
        }

        public async Task CreateGoalAsync(Guid userId, SavingGoalDto dto)
        {
            var goal = new SavingGoal
            {
                Id = Guid.NewGuid(),
                AppUserId = userId.ToString(),
                Name = dto.Name,
                TargetAmount = dto.TargetAmount,
                CurrentAmount = dto.CurrentAmount
            };

            await _repository.AddGoalAsync(goal);
        }

        public async Task<bool> UpdateGoalAsync(Guid userId, Guid goalId, SavingGoalUpdateDto dto)
        {
            var goal = await _repository.GetGoalByIdAsync(goalId, userId);
            if (goal == null) return false;

            goal.Name = dto.Name;
            goal.TargetAmount = dto.TargetAmount;
            // Fix: Update CurrentAmount from DTO
            goal.CurrentAmount = dto.CurrentAmount;

            await _repository.UpdateGoalAsync(goal);
            return true;
        }

        public async Task<bool> DeleteGoalAsync(Guid userId, Guid goalId)
        {
            var goal = await _repository.GetGoalByIdAsync(goalId, userId);
            if (goal == null) return false;

            await _repository.DeleteGoalAsync(goal);
            return true;
        }
    }
}
