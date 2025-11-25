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
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key missing!");
            _httpClient = new HttpClient();
        }

        public async Task<List<string>> GenerateSuggestionsAsync(string userId)
        {
            try
            {
                // 1. Fetch Data
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

                if (transactions == null || !transactions.Any())
                    return new List<string> { "🤖 AI: Not enough data. Add income and expenses to get started." };

                // 2. Data Prep
                var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
                var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var totalSpent = expenses.Sum(t => t.Amount);
                var budgetLimit = budget?.LimitAmount ?? 0;

                // 3. Serialize Simplified Transactions for Context
                // We limit to top 30 expenses to avoid Token Limits, but include diverse data
                var simplifiedTransactions = expenses
                    .OrderByDescending(t => t.Amount)
                    .Take(30)
                    .Select(t => new
                    {
                        Date = t.TransactionDate.ToString("MM-dd"),
                        Cat = t.Category?.Name,
                        Merch = t.Merchant ?? "Unknown",
                        Amt = (int)t.Amount
                    });

                var transactionsJson = System.Text.Json.JsonSerializer.Serialize(simplifiedTransactions);

                // 4. Enhanced Prompt
                var promptText = $@"
                    You are a financial advisor. Analyze this user's last 30 days of data (Currency: HUF).
                    
                    SUMMARY:
                    - Income: {income:N0}
                    - Spent: {totalSpent:N0}
                    - Budget Limit: {budgetLimit:N0}
                    
                    TOP 30 TRANSACTIONS (JSON):
                    {transactionsJson}

                    INSTRUCTIONS:
                    Based *specifically* on the transaction patterns above, provide exactly 4 short, punchy insights.
                    
                    1. 🛑 **The 'Leak'**: Identify a specific merchant or category they are overspending on (e.g. 'You spent 40k at Tesco, try meal prepping').
                    2. 🔮 **Forecast**: Will they hit their {budgetLimit:N0} limit? Be direct.
                    3. 🧠 **Psychology**: Notice a habit? (e.g. 'You spend mostly on Fridays' or 'Lots of small coffee purchases').
                    4. 🚀 **Action**: One strict rule for next week.

                    Format as a simple list of strings. No markdown headers. Use emojis.";

                // 5. API Call
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}"; // Updated model version if available, or keep 1.5-flash
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                    return new List<string> { $"⚠️ AI Unavailable: {response.StatusCode}" };

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var responseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(responseText)) return new List<string> { "⚠️ AI returned no insights." };

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

        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}