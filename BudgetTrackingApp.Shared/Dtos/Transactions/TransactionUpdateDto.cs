using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Transactions
{
    public class TransactionUpdateDto
    {
        [Required(ErrorMessage = "Tranzakció összeg kötelező!")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public string? Description { get; set; }

        public DateTime TransactionDate { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
    }
}
