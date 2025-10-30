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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BudgetTrackerDbContext _context;

        public CategoryRepository(BudgetTrackerDbContext context)
        {
            _context = context;
        }
        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(string userId)
        {
            var categories= await _context.Categories.Where(c=>c.AppUserId==userId).ToListAsync();
            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            
        }

        public async Task<bool> IsCategoryOwnedByUserAsync(Guid categoryId, string userId)
        {
            return await _context.Categories.AnyAsync(c => c.Id == categoryId && c.AppUserId == userId);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);

            await _context.SaveChangesAsync();
        }
    }
}
