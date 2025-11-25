using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.AI;
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

        public async Task<AiAdviceDto> GenerateStructuredAdviceAsync(string userId)
        {
            var resultDto = new AiAdviceDto();

            try
            {
                // 1. Gather Data (Same as before)
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
                var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

                if (transactions == null || !transactions.Any())
                {
                    resultDto.ImmediateActions.Add("Please add some transactions first so I can analyze your data!");
                    return resultDto;
                }

                var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
                var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var totalSpent = expenses.Sum(t => t.Amount);
                var budgetLimit = budget?.LimitAmount ?? 0;

                // Simplified transaction list for the prompt to save tokens
                var txList = expenses.OrderByDescending(t => t.Amount).Take(40)
                    .Select(t => $"{t.TransactionDate:MM-dd}: {t.Merchant ?? "Unknown"} ({t.Category?.Name}) - {t.Amount} HUF");

                // 2. The "Super Prompt" requesting JSON
                var promptText = $@"
                    You are a financial expert. Analyze this user's 30-day data:
                    Income: {income} HUF, Spent: {totalSpent} HUF, Budget Limit: {budgetLimit} HUF.
                    Transactions: [ {string.Join(", ", txList)} ]

                    **TASK:**
                    Provide 5 distinct categories of advice. For EACH category, provide exactly 3-5 specific, actionable bullet points.
                    
                    **OUTPUT FORMAT:**
                    Return ONLY a valid JSON object matching this structure exactly (no markdown, no code blocks):
                    {{
                        ""SpendingPatterns"": [""...string..."", ""...string...""],
                        ""CostCutting"": [""...string..."", ""...string...""],
                        ""FutureForecast"": [""...string..."", ""...string...""],
                        ""SmartInvestments"": [""...string..."", ""...string...""],
                        ""ImmediateActions"": [""...string..."", ""...string...""]
                    }}

                    **CATEGORY GUIDES:**
                    1. SpendingPatterns: Identify habits (e.g., 'You buy coffee daily', 'Weekends are high spend').
                    2. CostCutting: Specific things to stop buying or buy cheaper.
                    3. FutureForecast: If they keep spending like this, what happens next month?
                    4. SmartInvestments: If savings > 0, suggest generic allocation (ETF/Bonds). If debt, suggest payoff method.
                    5. ImmediateActions: 5 concrete tasks for TODAY (e.g., 'Unsubscribe from Netflix', 'Cook dinner').
                    
                    Keep tone professional but ruthless.";

                // 3. Call Gemini
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                    var rawText = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                    if (!string.IsNullOrEmpty(rawText))
                    {
                        // Clean up potential markdown code blocks ```json ... ```
                        rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

                        try
                        {
                            var parsedAdvice = JsonSerializer.Deserialize<AiAdviceDto>(rawText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (parsedAdvice != null) resultDto = parsedAdvice;
                        }
                        catch
                        {
                            resultDto.SpendingPatterns.Add("Error parsing AI response. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultDto.ImmediateActions.Add($"System Error: {ex.Message}");
            }

            return resultDto;
        }

        // Internal Gemini classes (kept same)
        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}