using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class CommentairesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
