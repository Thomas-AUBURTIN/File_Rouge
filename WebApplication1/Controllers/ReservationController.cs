using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
