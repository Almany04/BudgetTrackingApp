using BudgetTrackingApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BudgetTrackingApp.Data.Entities
{
    public class Transactions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; private set; }
        [ForeignKey(nameof(User))]
        public int? UserId {  get; private set; }
        public int? CategoryId { get; private set; }
        public decimal? Amount { get; private set; }
        public TransactionType type{get; private set; }
        [StringLength(200)]
        public string? Description {  get; private set; }

        public DateTime? Date { get; set; }
        public DateTime? CreatedAt { get; set; }

        public bool IsRecurring { get; set; }
        public User? user { get; private set; }
    }
}
