using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class ShoppingListLogic : IShoppingListLogic
    {
        private readonly IShoppingItemRepository _repository;

        public ShoppingListLogic(IShoppingItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ShoppingItemDto>> GetListAsync(Guid userId)
        {
            var items = await _repository.GetItemsAsync(userId);
            return items.Select(i => new ShoppingItemDto
            {
                Id = i.Id,
                Name = i.Name,
                IsBought = i.IsBought
            }).ToList();
        }

        public async Task AddItemAsync(Guid userId, ShoppingItemDto dto)
        {
            var item = new ShoppingItem
            {
                Id = Guid.NewGuid(),
                AppUserId = userId.ToString(),
                Name = dto.Name,
                IsBought = false
            };
            await _repository.AddItemAsync(item);
        }

        public async Task ToggleItemAsync(Guid userId, Guid itemId, bool isBought)
        {
            var item = await _repository.GetItemByIdAsync(itemId, userId);
            if (item != null)
            {
                item.IsBought = isBought;
                await _repository.UpdateItemAsync(item);
            }
        }

        public async Task DeleteItemAsync(Guid userId, Guid itemId)
        {
            var item = await _repository.GetItemByIdAsync(itemId, userId);
            if (item != null)
            {
                await _repository.DeleteItemAsync(item);
            }
        }
    }
}
