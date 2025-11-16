using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Budget
{
    public class BudgetUpdateDto
    {
        [Required(ErrorMessage = "A keretösszeg megadása kötelező!")]
        [Range(0, double.MaxValue, ErrorMessage = "A keretösszeg nem lehet negatív!")]
        public decimal LimitAmount { get; set; }
    }
}
