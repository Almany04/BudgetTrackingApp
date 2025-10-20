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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; private set; }
        [Required]
        [ForeignKey(nameof(User))]
        public int? UserId { get; private set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal LimitAmount {  get; private set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal SpentAmount { get; private set;}
        public User? user { get; private set; }
    }
}
