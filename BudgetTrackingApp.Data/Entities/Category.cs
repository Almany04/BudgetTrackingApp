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
    public class Category
    {

        public Category()
        {
            Id = Guid.NewGuid();
            Transactions = new HashSet<Transactions>();

        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get;set; }
        [Required]
        public string AppUserId {  get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Transactions> Transactions { get; set; }
    }
}
