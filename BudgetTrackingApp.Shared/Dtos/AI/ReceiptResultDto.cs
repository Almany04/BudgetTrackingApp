using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.AI
{
    public class ReceiptResultDto
    {
        public string? Merchant { get; set; }
        public DateTime? Date { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = new();
        public string? DetectedPaymentMethod { get; set; }
    }
}
