using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebsite.Controllers
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
