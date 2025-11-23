using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetTrackingApp.Logic.Services
{
    public class AiSuggestionLogic : IAiSuggestionLogic
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AiSuggestionLogic(
            ITransactionRepository transactionRepository,
            IBudgetRepository budgetRepository,
            IConfiguration configuration)
        {
            _transactionRepository = transactionRepository;
            _budgetRepository = budgetRepository;
            // Retrieves key from secrets.json (local) or Azure App Settings (production)
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key missing!");
            _httpClient = new HttpClient();
        }

        public async Task<List<string>> GenerateSuggestionsAsync(string userId)
        {
            try
            {
                // 1. Fetch Financial Data (Last 30 Days)
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

                if (transactions == null || !transactions.Any())
                    return new List<string> { "🤖 AI: Not enough data. Add income and expenses to get started." };

                // 2. Calculate Financial Health
                var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
                var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var totalSpent = expenses.Sum(t => t.Amount);
                var budgetLimit = budget?.LimitAmount ?? 0;
                var savings = income - totalSpent;
                var savingsRate = income > 0 ? (savings / income) * 100 : 0;

                // Prepare Granular Data (Top Spending Areas)
                var topCategories = expenses
                    .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Take(5)
                    .Select(g => $"{g.Key} ({g.Sum(t => t.Amount):C0})");

                // 3. Advanced Strategic Prompt for Gemini 2.5 Flash
                // Note: Double quotes inside the $@ string are escaped as ""
                var promptText = $@"
                    Act as a ruthless Financial Strategist. Analyze this 30-day snapshot:
                    
                    FINANCIAL DATA:
                    - Income: {income:C0}
                    - Expenses: {totalSpent:C0}
                    - Net Savings: {savings:C0} (Savings Rate: {savingsRate:F1}%)
                    - Budget Limit: {budgetLimit:C0}
                    - Top Spending: {string.Join(", ", topCategories)}

                    TASK:
                    Provide 4 distinct, short sections using emojis:

                    1. 🔮 **Prediction**: Forecast my financial status next month based on these habits.
                    2. 📉 **Cut Costs**: Identify exactly one area to slash spending immediately.
                    3. 📈 **Investment Strategy**: 
                       - If savings > 0: Suggest a specific portfolio allocation (e.g., ""60% S&P500 ETF, 20% Bonds, 20% Crypto"") based on the savings amount of {savings:C0}.
                       - If savings <= 0: Give a harsh ""Debt Payoff"" plan.
                    4. 💡 **Immediate Action**: One task to perform today.

                    Keep it concise. No markdown bolding (**), just plain text.";

                // 4. Call Gemini 2.5 Flash API
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<string> { $"⚠️ AI Unavailable ({response.StatusCode}): Check Quota." };
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var responseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(responseText)) return new List<string> { "⚠️ AI returned no insights." };

                // Clean up response
                return responseText
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().Replace("*", "").Replace("#", ""))
                    .Where(s => s.Length > 5)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<string> { $"⚠️ Error: {ex.Message}" };
            }
        }

        // Internal DTOs for Gemini Response
        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}