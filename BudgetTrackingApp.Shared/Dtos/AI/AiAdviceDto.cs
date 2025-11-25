using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Shared.Dtos.AI
{
    public class AiAdviceDto
    {
        // The 5 Categories
        public List<string> SpendingPatterns { get; set; } = new(); 
        public List<string> CostCutting { get; set; } = new();      
        public List<string> FutureForecast { get; set; } = new();  
        public List<string> SmartInvestments { get; set; } = new();
        public List<string> ImmediateActions { get; set; } = new();
    }
}
