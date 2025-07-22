using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    public class CatalogueController : Controller
    {
        //// attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        public CatalogueController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
        public IActionResult Index(int i, string sort)
        {
            string query = "select J.titre,J.DESCRIPTION,J.IMAGE from jeux J ";
            List<Jeux> ListJeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                ListJeux = connexion.Query<Jeux>(query).ToList();
            }
            ViewData["i"] = i;
            return View(ListJeux);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Detail()
        {
            return View();
        }

    }
}
