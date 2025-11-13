using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Shared.Dtos.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface ICategoryLogic
    {
        Task<IEnumerable<CategoryViewDto>> GetCategoriesByUserIdAsync(string userId);
        Task<CategoryViewDto?> GetCategoryByIdAsync(Guid categoryId, string userId);
        Task CreateCategoryAsync(CategoryCreateDto categoryDto, string userId);
        Task UpdateCategoryAsync(Guid categoryId, CategoryUpdateDto categoryDto, string userId);
        Task DeleteCategoryAsync(Guid categoryId, string userId);
        Task<bool> IsCategoryOwnedByUserAsync(Guid categoryId, string userId);
       
    }
}
