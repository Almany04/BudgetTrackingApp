using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetTrackingApp.Shared.Dtos.Transactions
{
    public class BulkTransactionCreateDto
    {
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? Merchant { get; set; } // Shared for all items (e.g. Tesco)

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

        public List<TransactionItemDto> Items { get; set; } = new();
        public PaidBy PaidBy { get; set; } = PaidBy.Me;
        public bool IsSplit { get; set; } = false;
        public decimal MyShareRatio { get; set; } = 1.0m; // Default 100%
    }

    public class TransactionItemDto
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; } // e.g. "Milk"
        public Guid CategoryId { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Expense;
    }
}