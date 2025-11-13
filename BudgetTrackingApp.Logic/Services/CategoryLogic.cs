using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BudgetTrackingApp.Logic.Services
{
    public class CategoryLogic : ICategoryLogic
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CategoryLogic(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository= transactionRepository;
        }

        public async Task CreateCategoryAsync(CategoryCreateDto categoryDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                throw new Exception("A kategória neve nem lehet üres!");
            }
            var existingCategories=await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            bool nameExists=existingCategories.Any(c=>
                    c.Name.Equals(categoryDto.Name, StringComparison.OrdinalIgnoreCase)
            );

            if (nameExists)
            {
                throw new Exception($"Már létezik'{categoryDto.Name}' nevű kategória!");
            }

            var newCategoryEntity = new Category
            {
                Name = categoryDto.Name,
                AppUserId = userId,
            };

            await _categoryRepository.AddCategoryAsync(newCategoryEntity);
        }

        public async Task DeleteCategoryAsync(Guid categoryId, string userId)
        {
            var isOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!isOwned)
            {
                throw new Exception("Nincs jogosultsága törölni ezt a kategóriát!");
            }
            var hasTransaction = await _transactionRepository.HasTransactionsForCategoryAsync(categoryId);
            if (hasTransaction) 
            {
                throw new Exception("A kategória nem törölhető, mert tranzakciók hivatkoznak rá!");
            }

            var entityToDelete=await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (entityToDelete != null)
            {
                    await _categoryRepository.DeleteCategoryAsync(entityToDelete);
            }
        }

        public async Task<IEnumerable<CategoryViewDto>> GetCategoriesByUserIdAsync(string userId)
        {
            var categoryEntities= await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            var categoryDtos = categoryEntities.Select(entity => new CategoryViewDto
            {
                Id = entity.Id,
                Name = entity.Name
            });
            
            return categoryDtos;
        }

        public async Task<CategoryViewDto?> GetCategoryByIdAsync(Guid categoryId, string userId)
        {
            var isOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!isOwned)
            {
                return null;
            }
            var entity = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if(entity == null) return null;

            return new CategoryViewDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public async Task<bool> IsCategoryOwnedByUserAsync(Guid categoryId, string userId)
        {
            return await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
        }

        public async Task UpdateCategoryAsync(Guid categoryId, CategoryUpdateDto categoryDto, string userId)
        {
            var IsOwned=await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!IsOwned) 
            {
                throw new Exception("Nincs jogosultsága módosítani ezt a kategóriát!");
            }
            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                throw new Exception("A kategória neve nem lehet üres!");
            }

            var existingCategories=await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            bool nameCollision = existingCategories.Any(c => c.Name.Equals(categoryDto.Name, StringComparison.OrdinalIgnoreCase) && c.Id != categoryId);
            if (nameCollision)
            {
                throw new Exception($"Már létezik'{categoryDto.Name}' nevű kategória!");
            }

            var entityToUpdate = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (entityToUpdate == null)
            {
                throw new Exception("A kategória nem található.");
            }
            entityToUpdate.Name = categoryDto.Name;

            await _categoryRepository.UpdateCategoryAsync(entityToUpdate);

        }
    }
}
