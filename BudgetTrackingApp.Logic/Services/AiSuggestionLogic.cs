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
            // 1. Fetch Financial Data
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

            if (transactions == null || !transactions.Any())
                return new List<string> { "🤖 AI: Add some expenses to get personalized tips!" };

            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var totalSpent = expenses.Sum(t => t.Amount);
            var budgetLimit = budget?.LimitAmount ?? 0;

            // Group by Merchant and Category for better context
            var topMerchants = expenses
                .GroupBy(t => t.Merchant ?? "Unknown")
                .OrderByDescending(g => g.Sum(x => x.Amount))
                .Take(3)
                .Select(g => $"{g.Key} ({g.Sum(x => x.Amount):C0})");

            var categorySummary = expenses
                .GroupBy(t => t.Category?.Name ?? "Unknown")
                .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C0}")
                .ToList();

            // 2. Construct Prompt
            var promptText = $@"
                Act as a concise financial advisor. 
                User Data (Last 30 Days):
                - Total Budget: {budgetLimit:C0}
                - Total Spent: {totalSpent:C0}
                - Top Spending Places: {string.Join(", ", topMerchants)}
                - Category Breakdown: {string.Join(", ", categorySummary)}

                Task: Give 3 short, specific, and actionable financial tips based on this spending pattern.
                - If they spend a lot at one merchant, suggest alternatives.
                - Keep it friendly but professional.
                - No markdown formatting (bold/italic). 
                - Start each tip with a relevant emoji.";

            try
            {
                // Using Gemini 2.5 Pro for text generation
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                    return new List<string> { "⚠️ AI is currently unavailable (High Traffic)." };

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var responseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(responseText)) return new List<string>();

                return responseText
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => CleanText(s))
                    .Where(s => s.Length > 10)
                    .Take(3)
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