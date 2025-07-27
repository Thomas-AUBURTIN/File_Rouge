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
        @*
        public IActionResult Detail(int id)
        {
            string query = """
                select
                	j.* ,
                	t.*,
                	t2.*,
                	c.*,
                	u.nom 
                from
                	jeux j
                join jeux_type jt on
                	j.jeuid = jt.jeuid
                join types t on
                	t.typeid = jt.typeid
                join jeux_theme jt2 on
                	j.jeuid = jt2.jeuid
                join theme t2 on
                	t2.themeid = jt2.themeid
                join commentaires c on c.jeuid = j.jeuid 	
                join utilisateurs u on c.utilisateurid =u.utilisateurid 
                where
                	j.jeuid = @id;
                """;
            Jeux jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                jeux = connexion.Query<Jeux, Theme, Type, Commentaire, Jeux>(query,
                    (jeux, theme, type, commentaire) =>
                    {
                        jeux.types.Add(type);



                    }new { id = id },
        splitOn: "jeuid,typeid,themeid,jeuid,nom").First();
            }

        }
*@
        @*
        public IActionResult Index([FromQuery] string sort = "titre")
            {
                string route = Request.RouteValues.First().ToString();
                string query = "select * from livres left join livre_categorie on id = livre_categorie.livre_id left join categories on categorie_id = categories.id";
                List<Livre> livres;
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    livres = connexion.Query<Livre, Categorie, Livre>(query,
                    (livre, categorie) =>
                    {
                        if (categorie != null)
                        {

                            livre.categories.Add(categorie);
                        }
                        return livre;
                    }).ToList();
                }
                //LINQ
                livres = livres.GroupBy(l => l.id).Select(g =>
                    {
                        Livre groupedLivre = g.First();
                        if (groupedLivre.categories.Count > 0)
                        {
                            groupedLivre.categories = g.Select(l => l.categories.Single()).ToList();
                        }

                        return groupedLivre;
                    }).ToList();
                return View(livres);


            }
            public IActionResult Detail(int id)
{
    string query = "select e.id,e.date_emprunt,e.date_retour,l.titre,l.isbn,u.nom,u.prenom from emprunts e inner join livres l on e.livre_id = l.id inner join utilisateurs u on e.utilisateur_id = u.id where e.id=@id";

    Emprunt emprunt;

    using (var connexion = new NpgsqlConnection(_connexionString))
    {
        emprunt = connexion.Query<Emprunt, Livre, Utilisateur, Emprunt>(query,
        (emprunt, livre, utilisateur) =>
        {
            emprunt.livre = livre;
            emprunt.utilisateur = utilisateur;
            return emprunt;
        }, new { id = id },
        splitOn: "titre,nom").First();
    }

    return View(emprunt);
}




        *@



    }
}
