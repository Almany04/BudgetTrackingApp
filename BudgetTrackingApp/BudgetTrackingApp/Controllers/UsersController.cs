using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
