using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class ShoppingItem
    {
        public ShoppingItem() => Id = Guid.NewGuid();

        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsBought { get; set; }
        [Required]
        public string AppUserId { get; set; }
    }
}
