using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    public class ConnectionController : Controller
    {
        //// attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        public ConnectionController(IConfiguration configuration)
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
        public IActionResult Inscription([FromForm] Utilisateur utilisateur)
        {
            string query = "INSERT INTO utilisateurs (nom, email, motdepasse, telephone, dateinscription,administrateur) VALUES(@nom,@email,@motdepasse,@telephone,@dateinscription, false);";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    int nbLignesInserees = connexion.Execute(query, utilisateur);
                }
                catch (PostgresException e)
                {
                    if (e.ConstraintName!=null) { 
                    if (e.ConstraintName.Contains("email"))
                    {
                        ViewData["ValidateMessage"] = "Cet email est déjà utilisé.";
                    }
                    if (e.ConstraintName.Contains("nom"))
                    {
                        ViewData["ValidateMessage"] = "Ce nom est déjà utilisé.";
                    }
                    if (e.ConstraintName.Contains("mot"))
                    {
                        ViewData["ValidateMessage"] = "Ce nom est déjà utilisé.";
                    }
                }
                    return View("", utilisateur);
                }
                catch (Exception e)
                {
                    ViewData["ValidatMessage"] = "Erreur serveur. Veuillez réessayer ultérieurement. Si jamais ça continu contectez le support.";
                    // message d'erreur
                    return View("EditeurUtilisateur");
                }
            }


            return View("Connection");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
