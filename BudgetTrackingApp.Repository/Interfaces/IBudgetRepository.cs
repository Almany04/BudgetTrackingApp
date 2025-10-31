using BudgetTrackingApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Repository.Interfaces
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetBudgetByUserIdAsync(string userId);
        Task UpdateBudgetAsync(Budget budget);

    }
}
