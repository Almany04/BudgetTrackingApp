using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class BudgetLogic : IBudgetLogic
    {
        private readonly IBudgetRepository _budgetRepository;

        public BudgetLogic(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }
        public async Task<BudgetViewDto?> GetBudgetByUserIdAsync(string userId)
        {
            var entity= await _budgetRepository.GetBudgetByUserIdAsync(userId);

            if(entity == null)
            {
               return null;
            }
            var budgetview =new BudgetViewDto
            {
                LimitAmount = entity.LimitAmount,
                SpentAmount = entity.SpentAmount
            };
            return budgetview;
        }

        public async Task UpdateBudgetLimitAsync(BudgetUpdateDto budgetdto, string userId)
        {
            var entity = await _budgetRepository.GetBudgetByUserIdAsync(userId);
            if (entity == null)
            {
                throw new Exception("A felhasználóhoz tartozó budget nem található!");
            }
            entity.LimitAmount = budgetdto.LimitAmount;
            await _budgetRepository.UpdateBudgetAsync(entity);   
        }
    }
}
