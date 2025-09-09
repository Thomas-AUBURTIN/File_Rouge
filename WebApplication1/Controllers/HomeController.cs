using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [Route("/Home/HandleError/{statusCode}")]
        public IActionResult HandleError([FromRoute] int statusCode)
        {
            if (statusCode == 403)
            {
                return View("AccessDenied");
            }
            else if (statusCode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View("Error");
            }
        }


        public IActionResult Error([FromRoute] int statusCode)
        {
            return View();
        }
    }
}
