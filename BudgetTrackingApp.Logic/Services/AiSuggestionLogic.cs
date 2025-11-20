using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class AiSuggestionLogic : IAiSuggestionLogic
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBudgetRepository _budgetRepository;

        public AiSuggestionLogic(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository)
        {
            _transactionRepository = transactionRepository;
            _budgetRepository = budgetRepository;
        }

        public async Task<List<string>> GenerateSuggestionsAsync(string userId)
        {
            var suggestions = new List<string>();

            // 1. Fetch Data
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            var transactions = await _transactionRepository.GetTransactionsByUserIdFilteredAsync(userId, startDate, endDate);
            var budget = await _budgetRepository.GetBudgetByUserIdAsync(userId);

            if (budget == null) return new List<string> { "Állítsd be a havi keretedet a pontosabb tanácsokért!" };

            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var totalSpent = expenses.Sum(t => t.Amount);

            // 2. "AI" Rules Engine

            // Budget Health Check
            if (totalSpent > budget.LimitAmount)
            {
                suggestions.Add($"⚠️ FIGYELEM: Túllépted a havi keretedet {totalSpent - budget.LimitAmount:C0} összeggel! Próbálj meg visszafogni a nem létfontosságú kiadásokat.");
            }
            else if (totalSpent > budget.LimitAmount * 0.8m)
            {
                suggestions.Add("⚠️ Közeledsz a havi limitedhez (80%+). Legyél óvatos a hónap hátralévő részében.");
            }
            else
            {
                suggestions.Add("✅ Jól állsz a havi kereteddel. Így tovább!");
            }

            // Category Analysis
            var expensesByCategory = expenses
                .GroupBy(t => t.Category?.Name ?? "Egyéb")
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            if (expensesByCategory.Any())
            {
                var topCategory = expensesByCategory.First();
                suggestions.Add($"💡 Tudtad? A legtöbbet '{topCategory.Category}' kategóriában költötted ({topCategory.Total:C0}).");

                if (topCategory.Total > totalSpent * 0.4m)
                {
                    suggestions.Add($"📉 A kiadásaid 40%-a egyetlen kategóriába ({topCategory.Category}) megy. Érdemes lenne átvizsgálni ezeket a tételeket.");
                }
            }

            // Transaction Frequency
            if (expenses.Count > 20)
            {
                suggestions.Add("🔄 Sok apró tranzakciód volt ebben a hónapban. A sok kicsi sokra megy!");
            }

            if (suggestions.Count == 0)
            {
                suggestions.Add("🤖 Nincs elég adat a részletes elemzéshez. Rögzíts több tranzakciót!");
            }

            return suggestions;
        }
    }
}