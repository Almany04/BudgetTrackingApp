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
        public string? Merchant { get; set; }
        public string? Description { get; set; } 
        public string? SuggestedCategory { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
