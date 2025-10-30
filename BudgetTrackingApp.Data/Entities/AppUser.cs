using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class AppUser:IdentityUser
    {
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive {  get; set; }=true;
        [Required]
        public Guid BudgetId { get; set; }
       
        [ForeignKey("BudgetId")]
        public virtual Budget? Budget { get; set; }
        public virtual List<Transactions>? Transactions { get; set; } = new List<Transactions>();
        public virtual List<Category>? Categories { get; set; }=new List<Category>();

    }
}
