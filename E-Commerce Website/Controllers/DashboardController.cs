using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThemedApp.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
