using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using WebApplication1.Models;



namespace WebApplication1.Controllers
{
    public class Jeux_ThemeController : Controller
    {
        public IActionResult Index()
        {
            string query = "Select * from jeux_theme";
            List<Jeux_Type> Jeux_Types;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Jeux_Types = connexion.Query<Jeux_Type>(query).ToList();
            }
            return View(Jeux_Types);
        }
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;
        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public Jeux_ThemeController(IConfiguration configuration)
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
