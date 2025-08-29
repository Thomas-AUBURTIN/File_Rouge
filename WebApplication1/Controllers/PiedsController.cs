using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class PiedsController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult Contact()
        {
            return View();
        }
    }
}