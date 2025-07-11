using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    //ceci est un test git hub
    public class HomeController : Controller
    {
        //// attribut stockant la cha�ne de connexion � la base de donn�es
        private readonly string _connexionString;

        public HomeController(IConfiguration configuration)
        {
            // r�cup�ration de la cha�ne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            // si la cha�ne de connexionn'a pas �t� trouv� => d�clenche une exception => code http 500 retourn�
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
        public IActionResult Index()
        {

            string query = "select * from JEUX where jeuid=1 or jeuid=2 or jeuid=3 or jeuid=4 ;";
            List<Jeux> Jeux;
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    Jeux = connexion.Query<Jeux>(query).ToList();
                }
                return View(Jeux);
            }
            catch
            {
                return NotFound();
            }

        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Legal()
        {
            return View();
        }
        public IActionResult Inscription()
        {
            return View();
        }

        public IActionResult Home()
        {
            ViewData["page"] = "index";
            return View();
        }

        public IActionResult Catalogue()
        {
            ViewData["page"] = "others";
            return View();
        }

        public IActionResult Gestion()
        {
            ViewData["page"] = "others";
            return View();
        }

        public IActionResult Reservation()
        {
            ViewData["page"] = "others";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
