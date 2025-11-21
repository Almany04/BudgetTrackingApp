using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
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
            // 1. Fetch Data
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

            if (transactions == null || !transactions.Any())
                return new List<string> { "🤖 AI: I need more data! Add some transactions first." };

            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var totalSpent = expenses.Sum(t => t.Amount);
            var budgetLimit = budget?.LimitAmount ?? 0;

            var categorySummary = expenses
                .GroupBy(t => t.Category?.Name ?? "Unknown")
                .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C0}")
                .ToList();

            // 2. Build Prompt for Gemini 2.5 Pro (Thinking Model)
            var promptText = $@"
                You are an expert personal finance advisor. Analyze this user's monthly spending data carefully.
                
                Data:
                - Total Monthly Budget Limit: {budgetLimit:C0}
                - Total Spent So Far: {totalSpent:C0}
                - Spending Breakdown by Category: {string.Join(", ", categorySummary)}
                - Current Date: {DateTime.Now:yyyy-MM-dd}

                Task:
                Provide exactly 3 distinct, high-quality, and actionable financial tips.
                - Use your reasoning capabilities to detect subtle spending habits.
                - If they are over budget, be stern but helpful.
                - If they are under budget, suggest saving strategies or investment ideas.

                Format:
                Return ONLY the 3 tips separated by newlines. Do not use introductory text.
                Start each tip with an emoji suitable for the advice.";

            // 3. Call Gemini 2.5 Pro API
            try
            {
               
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = promptText } } }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                // 4. Parse Response
                var responseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(responseText)) return new List<string> { "🤖 AI: Could not generate advice this time." };

                var suggestions = responseText
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s.Length > 10)
                    .Take(3)
                    .Select(s => s.Trim())
                    .ToList();

                return suggestions;
            }
            catch (Exception ex)
            {
                return new List<string> { $"⚠️ AI Unavailable: {ex.Message}" };
            }
        }

        // --- DTOs ---
        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }
        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }
        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part>? Parts { get; set; }
        }
        private class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}