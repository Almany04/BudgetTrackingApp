using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IAiSuggestionLogic
    {
        // Only keep the text suggestion method
        Task<List<string>> GenerateSuggestionsAsync(string userId);
    }
}