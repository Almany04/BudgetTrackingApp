using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Budget
{
    public class BudgetViewDto
    {
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
    }
}
