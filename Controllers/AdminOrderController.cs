using Microsoft.AspNetCore.Mvc;

namespace FruityFresh.Controllers
{
    public class AdminOrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
