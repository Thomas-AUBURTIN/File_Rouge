using System.Data;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Npgsql;
using Dapper;


namespace WebApplication1.Controllers
{
    public class Jeux_TypeController : Controller
    {
        public IActionResult Index()
        {
            string query = "Select * from jeux_type";
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
        public Jeux_TypeController(IConfiguration configuration)
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
