using BudgetTrackingApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Repository.Interfaces
{
    public interface IShoppingItemRepository
    {
        Task<List<ShoppingItem>> GetItemsAsync(Guid userId);
        Task AddItemAsync(ShoppingItem item);
        Task UpdateItemAsync(ShoppingItem item);
        Task DeleteItemAsync(ShoppingItem item);
        Task<ShoppingItem?> GetItemByIdAsync(Guid id, Guid userId);
    }
}
