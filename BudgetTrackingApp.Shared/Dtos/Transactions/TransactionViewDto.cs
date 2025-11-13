using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
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

        public DateTime TransactionDate { get; set; }   

        public string CategoryName { get; set; }

    }
}
