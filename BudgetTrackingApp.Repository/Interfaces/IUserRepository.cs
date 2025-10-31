using BudgetTrackingApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(AppUser user, string password);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<AppUser?> GetUserByIdAsync(string userId);
        Task<AppUser?> GetUserByEmailAsync(string email);
    }
}
