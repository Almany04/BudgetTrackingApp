using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
