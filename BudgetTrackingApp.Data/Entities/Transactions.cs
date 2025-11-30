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
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card; 
        [StringLength(100)]
        public string? Merchant { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        public Guid? ReceiptId { get; set; }
        [Required]
        public string AppUserId { get; set; }
        public Guid CategoryId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public PaidBy PaidBy { get; set; } = PaidBy.Me;
        public bool IsSplit { get; set; } = false;
        [Column(TypeName = "decimal(5, 4)")]
        public decimal MyShareRatio { get; set; } = 0.5m;
        public Guid? SavingGoalId { get; set; }
        [ForeignKey("SavingGoalId")]
        public virtual SavingGoal? SavingGoal { get; set; }
    }
}
