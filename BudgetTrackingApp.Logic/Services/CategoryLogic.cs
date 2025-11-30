using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class CategoryLogic : ICategoryLogic
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CategoryLogic(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task CreateCategoryAsync(CategoryCreateDto categoryDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(categoryDto.Name)) throw new Exception("Name cannot be empty!");

            // SECURITY FIX: Verify Parent Category Ownership
            if (categoryDto.ParentCategoryId.HasValue)
            {
                var isParentOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryDto.ParentCategoryId.Value, userId);
                if (!isParentOwned)
                {
                    throw new UnauthorizedAccessException("Invalid Parent Category.");
                }
            }

            var existingCategories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            // Check for duplicates (considering parent scope)
            bool nameExists = existingCategories.Any(c =>
                c.Name.Equals(categoryDto.Name, StringComparison.OrdinalIgnoreCase) &&
                c.ParentCategoryId == categoryDto.ParentCategoryId);

            if (nameExists) throw new Exception($"Category '{categoryDto.Name}' already exists!");

            var newCategoryEntity = new Category
            {
                Name = categoryDto.Name,
                AppUserId = userId,
                ParentCategoryId = categoryDto.ParentCategoryId
            };

            await _categoryRepository.AddCategoryAsync(newCategoryEntity);
        }

        public async Task DeleteCategoryAsync(Guid categoryId, string userId)
        {
            var isOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!isOwned)
            {
                // SECURITY FIX: Use specific exception
                throw new UnauthorizedAccessException("Access denied.");
            }
            var hasTransaction = await _transactionRepository.HasTransactionsForCategoryAsync(categoryId);
            if (hasTransaction)
            {
                throw new Exception("Cannot delete category with existing transactions.");
            }

            var entityToDelete = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (entityToDelete != null)
            {
                await _categoryRepository.DeleteCategoryAsync(entityToDelete);
            }
        }

        public async Task<IEnumerable<CategoryViewDto>> GetCategoriesByUserIdAsync(string userId)
        {
            var categoryEntities = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            return categoryEntities.Select(entity => new CategoryViewDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentCategoryId = entity.ParentCategoryId,
                ParentCategoryName = entity.ParentCategory?.Name
            });
        }

        public async Task<CategoryViewDto?> GetCategoryByIdAsync(Guid categoryId, string userId)
        {
            var isOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!isOwned)
            {
                return null;
            }
            var entity = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (entity == null) return null;

            return new CategoryViewDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentCategoryId = entity.ParentCategoryId,
                ParentCategoryName = entity.ParentCategory?.Name
            };
        }

        public async Task<bool> IsCategoryOwnedByUserAsync(Guid categoryId, string userId)
        {
            return await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
        }

        public async Task UpdateCategoryAsync(Guid categoryId, CategoryUpdateDto categoryDto, string userId)
        {
            var IsOwned = await _categoryRepository.IsCategoryOwnedByUserAsync(categoryId, userId);
            if (!IsOwned)
            {
                throw new UnauthorizedAccessException("Access denied.");
            }
            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                throw new Exception("Category name cannot be empty.");
            }

            var existingCategories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            bool nameCollision = existingCategories.Any(c => c.Name.Equals(categoryDto.Name, StringComparison.OrdinalIgnoreCase) && c.Id != categoryId);
            if (nameCollision)
            {
                throw new Exception($"Category '{categoryDto.Name}' already exists!");
            }

            var entityToUpdate = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (entityToUpdate == null)
            {
                throw new Exception("Category not found.");
            }
            entityToUpdate.Name = categoryDto.Name;

            await _categoryRepository.UpdateCategoryAsync(entityToUpdate);

        }
    }
}