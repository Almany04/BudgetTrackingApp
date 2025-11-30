using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.AI
{
    public class ReceiptItemDto
    {
        public string ItemName { get; set; } // Full name e.g. "Friss Bazsalikom"
        public decimal Price { get; set; }

        // NEW: Hierarchical Suggestions
        public string SuggestedMainCategory { get; set; } // e.g. "Groceries" (Bevásárlás)
        public string SuggestedSubCategory { get; set; }  // e.g. "Vegetables" (Zöldség)

        public Guid? MatchedCategoryId { get; set; } // If we found the specific sub-category already
        public bool IsNewCategory { get; set; } = false;
    }
}
