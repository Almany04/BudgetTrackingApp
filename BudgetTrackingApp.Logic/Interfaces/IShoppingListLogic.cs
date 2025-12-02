using BudgetTrackingApp.Shared.Dtos.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IShoppingListLogic
    {
        Task<List<ShoppingItemDto>> GetListAsync(Guid userId);
        Task AddItemAsync(Guid userId, ShoppingItemDto dto);
        Task ToggleItemAsync(Guid userId, Guid itemId, bool isBought);
        Task DeleteItemAsync(Guid userId, Guid itemId);
    }
}
