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
        //// attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        public HomeController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
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
            
            return View();
        }

        public IActionResult Catalogue(int i)
        {
            {
                string query = "select J.titre,J.DESCRIPTION,J.IMAGE from jeux J ";
                List<Jeux> Jeux;
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    Jeux = connexion.Query<Jeux>(query).ToList();
                }
                ViewData["i"] = i;
                return View(Jeux);
            }
        }
        
            
        

        public IActionResult Gestion()
        {
            
            return View();
        }

        public IActionResult Reservation()
        {
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
