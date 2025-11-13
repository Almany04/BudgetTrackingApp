using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.Category
{
    public class CategoryCreateDto
    {
        [Required(ErrorMessage ="A kategoria név kötelező!")]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
