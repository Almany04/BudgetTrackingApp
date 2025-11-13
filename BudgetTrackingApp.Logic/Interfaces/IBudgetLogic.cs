using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Shared.Dtos.Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IBudgetLogic
    {
        Task<BudgetViewDto?> GetBudgetByUserIdAsync(string userId);
        Task UpdateBudgetLimitAsync(BudgetUpdateDto budgetdto, string userId);
    }
}
