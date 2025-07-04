using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UtilisateursController : Controller
    {
        public IActionResult Index()
        {
            string query = "Select * from utilisateurs";
            List<Utilisateur> Utilisateurs;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Utilisateurs = connexion.Query<Utilisateur>(query).ToList();
            }
            return View(Utilisateurs);
        }
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;
        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public UtilisateursController(IConfiguration configuration)
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
