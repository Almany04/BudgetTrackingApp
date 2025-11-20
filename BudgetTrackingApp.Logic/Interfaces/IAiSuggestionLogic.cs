using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Dtos.Budget;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IAiSuggestionLogic
    {
        Task<List<string>> GenerateSuggestionsAsync(string userId);
    }
}