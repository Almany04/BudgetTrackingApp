using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class Transactions
    {

        public Transactions()
        {
            Id = Guid.NewGuid();
            TransactionDate = DateTime.UtcNow;
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Required]
        public TransactionType Type {get; set; }
        [StringLength(500)]
        public string? Description {  get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [Required]
        public string AppUserId { get; set; }
        public Guid CategoryId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}
