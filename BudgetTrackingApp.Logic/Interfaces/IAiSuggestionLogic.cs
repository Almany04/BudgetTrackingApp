using BudgetTrackingApp.Shared.Dtos.AI;
using BudgetTrackingApp.Shared.Dtos.Budget;
using BudgetTrackingApp.Shared.Dtos.Transactions;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IAiSuggestionLogic
    {
        Task<List<string>> GenerateSuggestionsAsync(string userId);
        Task<ReceiptScanResultDto> ScanReceiptAsync(byte[] imageBytes, string contentType);
    }
}