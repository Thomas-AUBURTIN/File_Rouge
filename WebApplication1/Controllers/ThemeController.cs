using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ThemeController : Controller
    {
        public IActionResult Index()
        {
            string query = "Select * from theme";
            List<Theme> Themes;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Themes = connexion.Query<Theme>(query).ToList();
            }
            return View(Themes);
        }
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;
        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public ThemeController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
    }
}
