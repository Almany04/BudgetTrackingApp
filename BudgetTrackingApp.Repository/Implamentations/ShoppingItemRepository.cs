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
    public class ShoppingItemRepository : IShoppingItemRepository
    {
        private readonly BudgetTrackerDbContext _context;

        public ShoppingItemRepository(BudgetTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShoppingItem>> GetItemsAsync(Guid userId)
        {
            return await _context.ShoppingItems
                .Where(x => x.AppUserId == userId.ToString())
                .ToListAsync();
        }

        public async Task<ShoppingItem?> GetItemByIdAsync(Guid id, Guid userId)
        {
            return await _context.ShoppingItems
                .FirstOrDefaultAsync(x => x.Id == id && x.AppUserId == userId.ToString());
        }

        public async Task AddItemAsync(ShoppingItem item)
        {
            await _context.ShoppingItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(ShoppingItem item)
        {
            _context.ShoppingItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(ShoppingItem item)
        {
            _context.ShoppingItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
