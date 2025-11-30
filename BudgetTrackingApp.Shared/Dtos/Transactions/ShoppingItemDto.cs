using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Transactions
{
    public class ShoppingItemDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsBought { get; set; }
    }
}
