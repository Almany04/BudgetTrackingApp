using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
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
                // 1. Fetch Financial Data (Last 30 Days)
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

                if (transactions == null || !transactions.Any())
                    return new List<string> { "🤖 AI: Add some expenses to get personalized tips!" };

                var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
                var totalSpent = expenses.Sum(t => t.Amount);
                var budgetLimit = budget?.LimitAmount ?? 0;

                // 2. PREPARE DATA FOR AI (Now with Sub-Categories!)

                // Main Groups (e.g. Groceries)
                var mainGroups = expenses
                    .GroupBy(t => t.Category?.ParentCategory?.Name ?? t.Category?.Name ?? "Other")
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Take(5)
                    .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C0}");

                // Sub Items (e.g. Cheese, Vapsolo) - The "Real" spending
                var specificItems = expenses
                    .Where(t => t.Category?.ParentCategory != null) // Only look at sub-items
                    .GroupBy(t => t.Category?.Name ?? "Unknown")
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Take(5)
                    .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C0}");

                // Top Merchants
                var topMerchants = expenses
                    .GroupBy(t => t.Merchant ?? "Unknown")
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Take(3)
                    .Select(g => g.Key);

                // 3. SMART PROMPT
                var promptText = $@"
                    Act as a ruthless but helpful financial advisor.
                    
                    USER DATA (30 Days):
                    - Budget Limit: {budgetLimit:C0}
                    - Total Spent: {totalSpent:C0}
                    - Top Main Categories: {string.Join(", ", mainGroups)}
                    - Top Specific Items (The Problem Areas): {string.Join(", ", specificItems)}
                    - Favorite Shops: {string.Join(", ", topMerchants)}

                    TASK:
                    Give 5 specific, short, actionable tips to save money.
                    - Focus heavily on the 'Specific Items' (e.g. if they spend a lot on 'Cheese', tell them to buy generic brand).
                    - Mention specific shops if relevant.
                    - Make predictions about the future spending and how much the person can save.
                    - No markdown. Start each tip with an emoji.";

                // Using Gemini 2.5 Pro for quality
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                    return new List<string> { "⚠️ AI is currently unavailable." };

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var responseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(responseText)) return new List<string>();

                return responseText
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => CleanText(s))
                    .Where(s => s.Length > 10)
                    .Take(5) // Increased to 5 tips
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string> { "⚠️ AI connection error." };
            }
        }

        private string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var clean = input.Replace("**", "").Replace("*", "").Replace("#", "").Trim();
            if (clean.StartsWith("- ")) clean = clean.Substring(2);
            return clean;
        }

        // DTOs
        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}