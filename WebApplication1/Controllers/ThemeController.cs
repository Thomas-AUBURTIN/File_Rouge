using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class themeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
