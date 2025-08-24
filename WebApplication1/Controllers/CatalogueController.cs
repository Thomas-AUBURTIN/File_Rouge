using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.ViewModels;

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
            string query = "select J.titre,J.DESCRIPTION,J.IMAGE from jeux J order by J.TITRE";
            CatalogueViewModel catalogue_jeux = new CatalogueViewModel();
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                catalogue_jeux.ListJeux = connexion.Query<Jeux>(query).ToList();
            }

            catalogue_jeux = getTypesThemes(catalogue_jeux);

            ViewData["i"] = i;
            return View(catalogue_jeux);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult TrieType([FromForm] CatalogueViewModel typejeux)
        {
            CatalogueViewModel Jeutrie = new CatalogueViewModel();

            string querytype = "select J.*  from JEUX J join JEUX_TYPE JT on JT.JEUID  =J.JEUID  where JT.typeid = @id order by J.TITRE;";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    Jeutrie.ListJeux = connexion.Query<Jeux>(querytype, new { id = int.Parse(typejeux.SelectedType) }).ToList();


                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "pas de type ";
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }
            }
            ;
            Jeutrie = getTypesThemes(Jeutrie);
            ViewData["i"] = 0;
            return View("Index", Jeutrie);
        }

        public IActionResult TrieTheme([FromForm] CatalogueViewModel themejeux)
        {
            CatalogueViewModel Jeutrie = new CatalogueViewModel();

            string querytheme = "select J.*  from JEUX J join JEUX_Theme JT on JT.JEUID  =J.JEUID  where JT.themeid = @id order by J.TITRE;";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    Jeutrie.ListJeux = connexion.Query<Jeux>(querytheme, new { id = int.Parse(themejeux.SelectedTheme) }).ToList();


                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "pas de theme ";
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }
            }
            ;
            Jeutrie = getTypesThemes(Jeutrie);
            ViewData["i"] = 0;
            return View("Index", Jeutrie);
        }



        public CatalogueViewModel getTypesThemes(CatalogueViewModel listjeux)
        {




            {

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryThemes = "select distinct j.themeid , t.nom from jeux_theme j   join THEME t on j.themeid=t.themeid order by themeid ";
                    List<Theme> Themes = connexion.Query<Theme>(queryThemes).ToList();
                    foreach (Theme Theme in Themes)
                    {
                        listjeux.ListThemes.Add(new SelectListItem(Theme.nom, Theme.themeId.ToString()));
                    }

                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTypes = "select distinct j.TYPEID  , t.nom from JEUX_TYPE  j   join types t on j.typeid=t.typeid order by typeid ";
                    List<JeuxType> Types = connexion.Query<JeuxType>(queryTypes).ToList();
                    foreach (JeuxType type in Types)
                    {
                        listjeux.ListTypes.Add(new SelectListItem(type.nom, type.typeId.ToString()));
                    }


                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTemps = "select distinct tempsjeumoyen from JEUX order by TEMPSJEUMOYEN  ";
                    List<int> Temps = connexion.Query<int>(queryTemps).ToList();
                    foreach (int temp in Temps)
                    {
                        listjeux.ListTemps.Add(new SelectListItem(temp.ToString(), temp.ToString()));
                    }


                }
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryJoueurs = "select distinct NOMBREJOUEURSRECOMMANDES  from JEUX order by NOMBREJOUEURSRECOMMANDES   ";
                    List<int> Joueurs = connexion.Query<int>(queryJoueurs).ToList();
                    foreach (int Joueur in Joueurs)
                    {
                        listjeux.ListJoueur.Add(new SelectListItem(Joueur.ToString(), Joueur.ToString()));
                    }


                }
                return listjeux;


            }
        }
    }