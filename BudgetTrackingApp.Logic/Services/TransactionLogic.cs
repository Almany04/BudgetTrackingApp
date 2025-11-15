using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class TransactionLogic : ITransactionLogic
    {
        
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;

        public TransactionLogic(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IBudgetRepository budgetRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
        }
        public async Task CreateTransactionAsync(TransactionCreateDto transactiondto, string userId)
        {
            var valid = await _categoryRepository.IsCategoryOwnedByUserAsync(transactiondto.CategoryId, userId);
            if (!valid)
            {
                throw new Exception("Nem hozzá tartozó kategória!");
            }
            var newTransaction = new Transactions
            {
                Amount = transactiondto.Amount,
                TransactionDate= transactiondto.TransactionDate,
                Type = transactiondto.Type,
                Description= transactiondto.Description,
                CategoryId= transactiondto.CategoryId,
                AppUserId=userId,
                
            };
            await _transactionRepository.AddTransactionAsync(newTransaction);

            if (newTransaction.Type == TransactionType.Expense)
            {
                var budgetCurrent= await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budgetCurrent != null) 
                {
                    budgetCurrent.SpentAmount += transactiondto.Amount;
                    await _budgetRepository.UpdateBudgetAsync(budgetCurrent);
                }
                
            }
        }

        public async Task DeleteTransactionAsync(Guid Id, string userId)
        {
            var IsOwned=await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned)
            {
                throw new Exception("Nincs jogosultsága törölni ezt a tranzakciót!");
            }
            
            var transactionentity=await _transactionRepository.GetTransactionByIdAsync(Id);
            if (transactionentity==null) throw new Exception("Nincs ilyen tranzakció!");
            if (transactionentity?.Type==TransactionType.Expense)
            {
                var budgetCurrent = await _budgetRepository.GetBudgetByUserIdAsync(userId);
                if (budgetCurrent != null)
                {
                    budgetCurrent.SpentAmount -= transactionentity.Amount;
                    await _budgetRepository.UpdateBudgetAsync(budgetCurrent);
                }
            }
            await _transactionRepository.DeleteTransactionAsync(transactionentity);
        }

        public async Task<TransactionViewDto?> GetTransactionByIdAsync(Guid Id, string userId)
        {
            var IsOwned = await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned)
            {
               return null;
            }
            var entity = await _transactionRepository.GetTransactionByIdAsync(Id);
            if (entity == null) return null;
            return new TransactionViewDto
            {
                Id = entity.Id,
                Amount = entity.Amount,
                Description = entity.Description,
                TransactionDate = entity.TransactionDate,
                Type = entity.Type,
                CategoryName=entity.Category?.Name??"Ismeretlen",
                CategoryId = entity.CategoryId
            };
        }

        public async Task<IEnumerable<TransactionViewDto?>> GetTransactionsByUserIdFilteredAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transactionEntites=await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
            var transactionDto = transactionEntites.Select(entity => new TransactionViewDto
            {
                Id = entity.Id,
                Amount = entity.Amount,
                TransactionDate = entity.TransactionDate,
                Description = entity.Description,
                CategoryName=entity.Category?.Name ?? "Ismeretlen",
                Type = entity.Type,
                CategoryId = entity.CategoryId
            });

            return transactionDto;
            
        }

        public async Task UpdateTransactionAsync(Guid Id, TransactionUpdateDto transactiondto, string userId)
        {
            var IsOwned= await _transactionRepository.IsTransactionOwnedByIdAsync(Id, userId);
            if (!IsOwned)
            {
                throw new Exception("Nincs jogosultsága ezt a tranzakciót módosítani!");

            }
            var IsCategoryValid=await _categoryRepository.IsCategoryOwnedByUserAsync(transactiondto.CategoryId, userId);
            if (!IsCategoryValid)
            {
                throw new Exception("Érvénytelen kategóriára próbálja áthelyezni a tranzakciót!");
            }
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);
            var transactionToUpdate = await _transactionRepository.GetTransactionByIdAsync(Id);
            if (transactionToUpdate == null||budget==null)
            {
                throw new Exception("A tranzakció vagy a hozzá tartozó budget nem található.");
            }

            UpdateBudgetTransactionChange(budget,transactionToUpdate, transactiondto);

            transactionToUpdate.Amount = transactiondto.Amount;
            transactionToUpdate.TransactionDate = transactiondto.TransactionDate;
            transactionToUpdate.Description = transactiondto.Description;
            transactionToUpdate.Type = transactiondto.Type;
            transactionToUpdate.CategoryId = transactiondto.CategoryId;

            await _transactionRepository.UpdateTransactionAsync(transactionToUpdate);
            await _budgetRepository.UpdateBudgetAsync(budget);
        }

        private void UpdateBudgetTransactionChange(Budget budget, Transactions oldtransaction, TransactionUpdateDto newtransactionData)
        {
            if (oldtransaction.Type == TransactionType.Expense)
            {
                budget.SpentAmount-=oldtransaction.Amount;
            }

            if (newtransactionData.Type == TransactionType.Expense)
            {
                budget.SpentAmount+=newtransactionData.Amount;
            }
        }
    }
}
