using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Transactions
{
    public class TransactionViewDto
    {
        public Guid Id { get; set; }
        public decimal Amount {  get; set; }

        public TransactionType Type { get; set; }

        public string? Description { get; set; }
        [StringLength(100)]
        public string? Merchant { get; set; } 

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

        public DateTime TransactionDate { get; set; }   

        public string CategoryName { get; set; }
        public Guid CategoryId { get; set; }

        public Guid? ReceiptId { get; set; }
        public PaidBy PaidBy { get; set; }
        public bool IsSplit { get; set; }
        public Guid? SavingGoalId { get; set; }
        public decimal MyShareRatio { get; set; } = 0.5m;
        public string? SavingGoalName { get; set; }

    }
}
