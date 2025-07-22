using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

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
