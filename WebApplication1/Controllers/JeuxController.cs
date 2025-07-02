using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class JeuxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
