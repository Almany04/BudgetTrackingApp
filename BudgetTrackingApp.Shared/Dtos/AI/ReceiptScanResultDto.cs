using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.AI
{
    public class ReceiptScanResultDto
    {
        public decimal? Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? Description { get; set; } // Pl. a bolt neve vagy a tétel
        public string? SuggestedCategory { get; set; } // Az AI tippje (pl. "Élelmiszer")
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
