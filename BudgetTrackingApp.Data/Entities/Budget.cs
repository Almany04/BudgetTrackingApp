using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class Budget
    {
        public Budget()
        {
            Id= Guid.NewGuid();
            LimitAmount = 0;
            SpentAmount = 0;
        }
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string AppUserId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal LimitAmount {  get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal SpentAmount { get; set;}
        public virtual AppUser? AppUser { get; set; }
    }
}
