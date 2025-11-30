using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.AI;
using BudgetTrackingApp.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class AiSuggestionLogic : IAiSuggestionLogic
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AiSuggestionLogic(
            ITransactionRepository transactionRepository,
            IBudgetRepository budgetRepository,
            ICategoryRepository categoryRepository,
            IConfiguration configuration)
        {
            _transactionRepository = transactionRepository;
            _budgetRepository = budgetRepository;
            _categoryRepository = categoryRepository;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key missing!");
            _httpClient = new HttpClient();
        }

        // ---------------------------------------------------------
        // 1. RECEIPT SCANNER (New Feature - Gemini 2.5 Pro)
        // ---------------------------------------------------------
        public async Task<ReceiptResultDto> ScanReceiptAsync(string base64Image, string userId)
        {
            // 1. Get existing categories
            var userCategories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);
            var catList = userCategories.Select(c =>
                c.ParentCategory != null
                ? $"{c.ParentCategory.Name} > {c.Name}"
                : c.Name
            ).ToList();

            var categoryContext = string.Join(", ", catList);

            // 2. UPDATED Prompt for Discounts & Specific Subcategories
            var promptText = $@"
                You are an expert Hungarian cashier. Analyze this receipt image pixel-perfectly.

                **CONTEXT:**
                - Existing Categories: [{categoryContext}]
                - Currency: HUF
                
                **TASKS:**
                1. **Merchant & Date:** Extract Store Name and Date.
                2. **Payment Method:** Look for keywords at the bottom:
                   - 'BANKKÁRTYA', 'VISA', 'MASTERCARD' -> Output: 'Card'
                   - 'KÉSZPÉNZ' -> Output: 'Cash'
                   - 'SZÉP KÁRTYA' -> Output: 'SzepCard'
                   - Default if unknown: 'Card'

                3. **Item Extraction (CRITICAL - HANDLE DISCOUNTS):** - List every purchased item line.
                   - **DISCOUNT RULE:** If a line contains 'DIVERZUM', 'KEDVEZMÉNY', 'AKCIÓ' or a negative number (e.g. '-180') immediately below a product, **YOU MUST SUBTRACT IT** from the product's price.
                   - Example: 
                     Line 1: 'SAJT ... 899'
                     Line 2: 'eB00 DIVERZUM ... -180'
                     --> Result Item Price: 719 (899 - 180).
                   - **CLEANING:** Remove '¤', 'Ft', 'A', 'B', 'C' flags from names.

                4. **Categorization:** Map to existing categories or invent a logical Hungarian Main/Sub category.

                **OUTPUT JSON ONLY:**
                {{
                    ""Merchant"": ""Store Name"",
                    ""Date"": ""2025-11-20"",
                    ""DetectedPaymentMethod"": ""Card"",
                    ""Items"": [
                        {{ ""ItemName"": ""Sajt"", ""Price"": 719, ""SuggestedMainCategory"": ""Élelmiszer"", ""SuggestedSubCategory"": ""Tejtermék"" }}
                    ]
                }}
            ";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = promptText },
                            new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new Exception("Gemini 2.5 Pro not found. Try 'gemini-1.5-pro'.");
                throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
            }

            var resultDto = new ReceiptResultDto();
            var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var rawText = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (!string.IsNullOrEmpty(rawText))
            {
                var cleanedJson = rawText.Replace("```json", "").Replace("```", "").Trim();
                try
                {
                    resultDto = JsonSerializer.Deserialize<ReceiptResultDto>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { /* Parsing failed */ }
            }

            // 5. Post-Processing: Match against DB
            foreach (var item in resultDto.Items)
            {
                // Clean symbol if AI missed it
                item.ItemName = item.ItemName.Replace("¤", "").Trim();

                var matchedSub = userCategories.FirstOrDefault(c =>
                    c.Name.Equals(item.SuggestedSubCategory, StringComparison.OrdinalIgnoreCase) &&
                    c.ParentCategory != null &&
                    c.ParentCategory.Name.Equals(item.SuggestedMainCategory, StringComparison.OrdinalIgnoreCase)
                );

                if (matchedSub != null)
                {
                    item.MatchedCategoryId = matchedSub.Id;
                    item.IsNewCategory = false;
                }
                else
                {
                    item.MatchedCategoryId = null;
                    item.IsNewCategory = true;
                }
            }

            return resultDto;
        }

       

        // ---------------------------------------------------------
        // 2. FINANCIAL ADVISOR (Restored Feature - Gemini 2.0 Flash)
        // ---------------------------------------------------------
        public async Task<AiAdviceDto> GenerateStructuredAdviceAsync(string userId)
        {
            var resultDto = new AiAdviceDto();

            try
            {
                // Gather Data
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

                // Simplified list for token efficiency
                var txList = expenses.OrderByDescending(t => t.Amount).Take(40)
                    .Select(t => $"{t.TransactionDate:MM-dd}: {t.Merchant ?? "Unknown"} ({t.Category?.Name}) - {t.Amount} HUF");

                var promptText = $@"
                    You are a financial expert. Analyze this user's 30-day data:
                    Income: {income} HUF, Spent: {totalSpent} HUF, Budget Limit: {budgetLimit} HUF.
                    Transactions: [ {string.Join(", ", txList)} ]

                    **TASK:**
                    Provide 5 distinct categories of advice. For EACH category, provide exactly 3-5 specific, actionable bullet points.
                    
                    **OUTPUT FORMAT:**
                    Return ONLY a valid JSON object matching this structure exactly (no markdown):
                    {{
                        ""SpendingPatterns"": [""...string..."", ""...string...""],
                        ""CostCutting"": [""...string..."", ""...string...""],
                        ""FutureForecast"": [""...string..."", ""...string...""],
                        ""SmartInvestments"": [""...string..."", ""...string...""],
                        ""ImmediateActions"": [""...string..."", ""...string...""]
                    }}
                ";

                // Using Gemini 2.0 Flash for text generation (efficient & fast)
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = promptText } } } } };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                    var rawText = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                    if (!string.IsNullOrEmpty(rawText))
                    {
                        var rawJson = rawText.Replace("```json", "").Replace("```", "").Trim();
                        try
                        {
                            var parsedAdvice = JsonSerializer.Deserialize<AiAdviceDto>(rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (parsedAdvice != null) resultDto = parsedAdvice;
                        }
                        catch
                        {
                            resultDto.SpendingPatterns.Add("Error parsing AI response. Please try again.");
                        }
                    }
                }
                else
                {
                    resultDto.ImmediateActions.Add("AI Service is currently unavailable.");
                }
            }
            catch (Exception ex)
            {
                resultDto.ImmediateActions.Add($"System Error: {ex.Message}");
            }

            return resultDto;
        }

        // ---------------------------------------------------------
        // INTERNAL HELPERS (Shared for both methods)
        // ---------------------------------------------------------
        private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
        private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
        private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
        private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
    }
}