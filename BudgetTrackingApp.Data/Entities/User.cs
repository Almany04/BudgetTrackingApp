using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data.Entities
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; private set; }
        [StringLength(100)]
        public string? UserName { get; private set; }
        [StringLength(200)]
        [Required]
        public string? Email { get; private set; }

        [StringLength(200)]
        [Required]
        public string? Password { get; private set; }

        public DateTime? CreatedAt { get; private set; }

        public bool IsActive {  get; private set; }
        [Required]
        public Budget? Budget { get; set; }
        public List<Transactions>? Transactions { get; set; }
        public List<Category>? Categories { get; set; }

    }
}
