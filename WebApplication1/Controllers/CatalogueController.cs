using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.ViewModels;
using System.Data;

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
            string query = "select J.* from jeux J order by J.TITRE";
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
        [HttpPost]
        public IActionResult Trie([FromForm] CatalogueViewModel typejeux)
        {
            CatalogueViewModel Jeutrie = new CatalogueViewModel();

            string querytrie = "select distinct j.*   from JEUX j  join JEUX_THEME jt1 on j.JEUID = jt1.JEUID  join THEME T1 on t1.THEMEID  = jt1.THEMEID  join JEUX_type jt2 on j.JEUID = jt2.JEUID  join Types T2 on t2.TYPEID = jt2.TYPEID  where";
            bool first = true;
            if (typejeux.SelectedTheme != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " t1.themeid = @theme ";
            }
            ;
            if (typejeux.SelectedType != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " t2.typeid = @type ";
            }
            ;
            if (typejeux.SelectedJoueur != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " j.nombrejoueursrecommandes = @joueur ";
            }
            ;
            if (typejeux.Selectedtemps != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " j.tempsjeumoyen = @temps ";
            }
            ;
            querytrie += "  order by j.titre; ";




            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    Jeutrie.ListJeux = connexion.Query<Jeux>(querytrie, new
                    {
                        theme = string.IsNullOrEmpty(typejeux.SelectedTheme) ? (int?)null : int.Parse(typejeux.SelectedTheme),
                        type = string.IsNullOrEmpty(typejeux.SelectedType) ? (int?)null : int.Parse(typejeux.SelectedType),
                        joueur = string.IsNullOrEmpty(typejeux.SelectedJoueur) ? (int?)null : int.Parse(typejeux.SelectedJoueur),
                        temps = string.IsNullOrEmpty(typejeux.Selectedtemps) ? (int?)null : int.Parse(typejeux.Selectedtemps)
                    }).ToList();


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





        public CatalogueViewModel getTypesThemes(CatalogueViewModel listjeux)
        {



            listjeux.ListThemes.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListTypes.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListJoueur.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListTemps.Add(new SelectListItem("--Choix tu trie--", ""));


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
        public IActionResult Detail(int id)
        {

            Jeux jeu;
            string queryjeux = "select j.*,t1.*,t2.* ,c.* from jeux j join JEUX_THEME JT1 on j.JEUID = jt1.JEUID join theme t1 on t1.THEMEID  = jt1.THEMEID join JEUX_TYPE JT2 on j.JEUID = jt2.JEUID join types t2 on t2.typeID  = jt2.typeID join COMMENTAIRES C  on c.JEUID = j.JEUID  where j.JEUID = @id";
            List<Jeux> jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {

                jeux = connexion.Query<Jeux, Theme, JeuxType, Commentaire, Jeux>(queryjeux,
                (jeu, theme, type, commentaire) =>
                {
                    jeu.themes.Add(theme);
                    jeu.types.Add(type);
                    jeu.commentaires.Add(commentaire);

                    return jeu;
                }, new { id = id },
                splitOn: "themeid,typeid,jeuid"
                ).ToList();

                }catch(System.Exception ex)
                {
                    return NotFound();
                }

                jeu = jeux.GroupBy(j => j.jeuid).Select(g =>
                {
                    Jeux groupedJeu = g.First();
                    if (groupedJeu.commentaires.Count != 0)
                    {
                        groupedJeu.commentaires = g.Select(j => j.commentaires.Single())
                        .DistinctBy((c) => c.jeuId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                    .ToList();
                    }
                    if (groupedJeu.types.Count != 0)
                    {
                        groupedJeu.types = g.Select(j => j.types.Single())
                                                    .DistinctBy((c) => c.typeId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                    .ToList();

                    }
                    if (groupedJeu.themes.Count != 0)
                    {
                        groupedJeu.themes = g.Select(j => j.themes.Single())
                                                    .DistinctBy((c) => c.themeId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                    .ToList();

                    }
                    return groupedJeu;
                }).First();
                return View(jeu);
            }
        }
        public EditeurJeuViewModel getTypesThemes(EditeurJeuViewModel singleJeux)
        {



            singleJeux.ListThemes.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListTypes.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListJoueur.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListTemps.Add(new SelectListItem("--Choix tu trie--", ""));


            {

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryThemes = "select distinct j.themeid , t.nom from jeux_theme j   join THEME t on j.themeid=t.themeid order by themeid ";
                    List<Theme> Themes = connexion.Query<Theme>(queryThemes).ToList();
                    foreach (Theme Theme in Themes)
                    {
                        singleJeux.ListThemes.Add(new SelectListItem(Theme.nom, Theme.themeId.ToString()));
                    }

                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTypes = "select distinct j.TYPEID  , t.nom from JEUX_TYPE  j   join types t on j.typeid=t.typeid order by typeid ";
                    List<JeuxType> Types = connexion.Query<JeuxType>(queryTypes).ToList();
                    foreach (JeuxType type in Types)
                    {
                        singleJeux.ListTypes.Add(new SelectListItem(type.nom, type.typeId.ToString()));
                    }


                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTemps = "select distinct tempsjeumoyen from JEUX order by TEMPSJEUMOYEN  ";
                    List<int> Temps = connexion.Query<int>(queryTemps).ToList();
                    foreach (int temp in Temps)
                    {
                        singleJeux.ListTemps.Add(new SelectListItem(temp.ToString(), temp.ToString()));
                    }


                }
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryJoueurs = "select distinct NOMBREJOUEURSRECOMMANDES  from JEUX order by NOMBREJOUEURSRECOMMANDES   ";
                    List<int> Joueurs = connexion.Query<int>(queryJoueurs).ToList();
                    foreach (int Joueur in Joueurs)
                    {
                        singleJeux.ListJoueur.Add(new SelectListItem(Joueur.ToString(), Joueur.ToString()));
                    }


                }
                return singleJeux;


            }
        }
        public IActionResult Detail2(int id)
        {

            Jeux jeu;
            string queryjeux = "select j.*,t1.*,t2.* ,c.* from jeux j join JEUX_THEME JT1 on j.JEUID = jt1.JEUID join theme t1 on t1.THEMEID  = jt1.THEMEID join JEUX_TYPE JT2 on j.JEUID = jt2.JEUID join types t2 on t2.typeID  = jt2.typeID join COMMENTAIRES C  on c.JEUID = j.JEUID  where j.JEUID = @id";
            List<Jeux> jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {

                    jeux = connexion.Query<Jeux, Theme, JeuxType, Commentaire, Jeux>(queryjeux,
                    (jeu, theme, type, commentaire) =>
                    {
                        jeu.themes.Add(theme);
                        jeu.types.Add(type);
                        jeu.commentaires.Add(commentaire);

                        return jeu;
                    }, new { id = id },
                    splitOn: "themeid,typeid,jeuid"
                    ).ToList();

                }
                catch (System.Exception ex)
                {
                    return NotFound();
                }

                jeu = jeux.GroupBy(j => j.jeuid).Select(g =>
            {
                Jeux groupedJeu = g.First();
                if (groupedJeu.commentaires.Count != 0)
                {
                    groupedJeu.commentaires = g.Select(j => j.commentaires.Single())
                    .DistinctBy((c) => c.jeuId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                .ToList();
                }
                if (groupedJeu.types.Count != 0)
                {
                    groupedJeu.types = g.Select(j => j.types.Single())
                                                .DistinctBy((c) => c.typeId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                .ToList();

                }
                if (groupedJeu.themes.Count != 0)
                {
                    groupedJeu.themes = g.Select(j => j.themes.Single())
                                                .DistinctBy((c) => c.themeId) // pour ne pas avoir de doublon sur la catégories on le distinct by 
                                                .ToList();

                }
                return groupedJeu;
            }).First();
            return View(jeu);
        }
    }

}
}
