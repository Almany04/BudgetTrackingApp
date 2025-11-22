using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.AI;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
                return new List<string> { "🤖 AI: Add some transactions to get insights!" };

            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var totalSpent = expenses.Sum(t => t.Amount);
            var budgetLimit = budget?.LimitAmount ?? 0;

            var categorySummary = expenses
                .GroupBy(t => t.Category?.Name ?? "Unknown")
                .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C0}")
                .ToList();

            // 2. Prompt for Gemini 1.5 Flash (Fast & Clean)
            var promptText = $@"
                Act as a financial advisor. Data:
                - Budget: {budgetLimit:C0}
                - Spent: {totalSpent:C0}
                - Breakdown: {string.Join(", ", categorySummary)}

                Task: 3 short, actionable financial tips (Plain text, no markdown, no bolding).
                Start each with an emoji.";

            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode) return new List<string> { "⚠️ AI Service Unavailable." };

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
            catch (Exception) { return new List<string> { "⚠️ AI is unreachable." }; }
        }

        private string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            // Remove Markdown symbols
            var clean = input.Replace("**", "").Replace("*", "").Replace("#", "").Replace("`", "").Trim();
            if (clean.StartsWith("- ")) clean = clean.Substring(2);
            return clean;
        }

        public async Task<ReceiptScanResultDto> ScanReceiptAsync(byte[] imageBytes, string contentType)
        {
            try
            {
                string base64Image = Convert.ToBase64String(imageBytes);

                // IMPROVED PROMPT: specifically tailored for Hungarian Receipts (SPAR, etc.)
                var promptText = @"
                    Analyze this receipt image (likely Hungarian). Extract raw JSON with these exact fields:
                    - ""Merchant"": The store name at the top (e.g., SPAR, Tesco, Rossmann).
                    - ""TransactionDate"": Date in YYYY-MM-DD format. Look for formats like '2025.11.20' or '2025.11.20.'.
                    - ""Amount"": The total amount paid. Look for 'Összesen', 'Összeg', or 'Bankkártya'. Ignore VAT lines (ÁFA). Return as a number.
                    - ""Description"": A short summary of the items purchased (e.g., 'Tej, Kenyér, Hús').
                    - ""SuggestedCategory"": A general category guess based on the items (e.g., 'Élelmiszer', 'Bevásárlás', 'Utazás').
                    
                    Return ONLY valid JSON. Do NOT wrap in markdown code blocks.";

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[] {
                        new {
                            parts = new object[] {
                                new { text = promptText },
                                new { inline_data = new { mime_type = contentType, data = base64Image } }
                            }
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return new ReceiptScanResultDto { IsSuccess = false, ErrorMessage = $"AI Error: {response.StatusCode}" };
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var jsonText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(jsonText)) return new ReceiptScanResultDto { IsSuccess = false, ErrorMessage = "No text found in receipt." };

                // CLEANUP: Remove ```json and ``` wrappers if the AI adds them
                jsonText = Regex.Replace(jsonText, @"^```json\s*", "", RegexOptions.IgnoreCase);
                jsonText = Regex.Replace(jsonText, @"^```\s*", "", RegexOptions.IgnoreCase);
                jsonText = Regex.Replace(jsonText, @"\s*```$", "", RegexOptions.IgnoreCase);
                jsonText = jsonText.Trim();

                try
                {
                    var dto = JsonSerializer.Deserialize<ReceiptScanResultDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        dto.IsSuccess = true;
                        return dto;
                    }
                }
                catch (JsonException)
                {
                    return new ReceiptScanResultDto { IsSuccess = false, ErrorMessage = "AI format error (JSON)." };
                }

                return new ReceiptScanResultDto { IsSuccess = false };
            }
            catch (Exception ex)
            {
                return new ReceiptScanResultDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        // --- DTOs for Gemini API ---
        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}