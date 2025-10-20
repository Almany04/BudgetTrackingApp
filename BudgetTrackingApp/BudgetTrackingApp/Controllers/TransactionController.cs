using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
