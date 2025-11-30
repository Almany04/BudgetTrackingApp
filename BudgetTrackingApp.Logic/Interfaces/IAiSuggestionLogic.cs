using BudgetTrackingApp.Shared.Dtos.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IAiSuggestionLogic
    {
        // Only keep the text suggestion method
        Task<AiAdviceDto> GenerateStructuredAdviceAsync(string userId);
        Task<ReceiptResultDto> ScanReceiptAsync(string base64Image, string userId);
    }
}