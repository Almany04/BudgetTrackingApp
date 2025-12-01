using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.User
{
    public class SavingGoalUpdateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        // Added to support updating the current saved amount manually
        public decimal CurrentAmount { get; set; }

    }
}
