using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.Models;
using Dapper;

namespace WebApplication1.Controllers
{
    public class JeuxController : Controller
    {
        public IActionResult Index()
        {
            string query = "Select * from jeux";
            List<Jeux> Jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Jeux = connexion.Query<Jeux>(query).ToList();
            }
            return View(Jeux);
        }
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;
        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public JeuxController(IConfiguration configuration)
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
