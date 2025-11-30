using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class SavingGoal
    {
        public SavingGoal() => Id = Guid.NewGuid();

        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }
        [Required]
        public string AppUserId { get; set; }

        public virtual ICollection<Transactions> Transactions { get; set; }
    }
}
