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
                        bool selected = singleJeux.Jeu.idTheme.Contains(Theme.themeId);
                        singleJeux.ListThemes.Add(new SelectListItem(Theme.nom, Theme.themeId.ToString(), selected));
                    }

                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTypes = "select distinct j.TYPEID  , t.nom from JEUX_TYPE  j   join types t on j.typeid=t.typeid order by typeid ";
                    List<JeuxType> Types = connexion.Query<JeuxType>(queryTypes).ToList();
                    foreach (JeuxType type in Types)
                    {
                        bool selected = singleJeux.Jeu.idType.Contains(type.typeId) ? true : false;
                        singleJeux.ListTypes.Add(new SelectListItem(type.nom, type.typeId.ToString(), selected));
                    }


                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTemps = "select distinct tempsjeumoyen from JEUX order by TEMPSJEUMOYEN  ";
                    List<int> Temps = connexion.Query<int>(queryTemps).ToList();
                    foreach (int temp in Temps)
                    {
                        bool selected = singleJeux.Jeu.tempsjeumoyen == temp ? true : false;
                        singleJeux.ListTemps.Add(new SelectListItem(temp.ToString(), temp.ToString(), selected));
                    }


                }
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryJoueurs = "select distinct NOMBREJOUEURSRECOMMANDES  from JEUX order by NOMBREJOUEURSRECOMMANDES   ";
                    List<int> Joueurs = connexion.Query<int>(queryJoueurs).ToList();
                    foreach (int Joueur in Joueurs)
                    {
                        bool selected = singleJeux.Jeu.nombrejoueursrecommandes == Joueur ? true : false;
                        singleJeux.ListJoueur.Add(new SelectListItem(Joueur.ToString(), Joueur.ToString(), selected));
                    }


                }
                return singleJeux;


            }
        }
        private string ManageCover(IFormFile file)
        {

            // vérification de la validité de l'image fournie pour la couverture
            string[] permittedExtensions = { ".jpeg", ".jpg", ".png", ".gif" };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                // cette ligne permet de mettre le message d'erreur au bon endroit dans la vue (c.a.d à côté du file picker)
                ModelState["jeu.imageFile"]!.Errors.Add(new ModelError("Ce type de fichier n'est pas accepté."));
                throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
            }

            //  enregistrement de l'image sur le système de fichiers et création du chemin de l'image afin de l'enregistrer en BDD
            string? filePath = Path.Combine("/img/jeu/",
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName)).ToString();

            using (var stream = System.IO.File.Create("wwwroot" + filePath))
            {
                file.CopyTo(stream);
            }
            return filePath;
        }
        public IActionResult editer(int id)
        {

            using (var connexion = new NpgsqlConnection(_connexionString))

            {
                Jeux groupedJeux = null;
                EditeurJeuViewModel edit = new() { action = "Editer", titre = "Modification jeu", idJeu = id };
                string queryjeux = "select j.*,t1.*,t2.*  from jeux j join JEUX_THEME JT1 on j.JEUID = jt1.JEUID join theme t1 on t1.THEMEID  = jt1.THEMEID join JEUX_TYPE JT2 on j.JEUID = jt2.JEUID join types t2 on t2.typeID  = jt2.typeID  where j.JEUID = @id";
                List<Jeux> jeux;
                try
                {

                    jeux = connexion.Query<Jeux, Theme, JeuxType, Jeux>(queryjeux,
                    (jeu, theme, type) =>
                    {
                        jeu.themes.Add(theme);
                        jeu.types.Add(type);
                        jeu.idType.Add(type.typeId);
                        jeu.idTheme.Add(theme.themeId);

                        return jeu;
                    }, new { id = id },
                    splitOn: "themeid,typeid"
                    ).ToList();

                }
                catch (System.Exception ex)
                {
                    return NotFound();
                }

                edit.Jeu = jeux.GroupBy(l => l.jeuid).Select(g =>
                {
                    groupedJeux = g.First();
                    groupedJeux.themes = g.Select(l => l.themes.First()).ToList();
                    groupedJeux.types = g.Select(l => l.types.First()).ToList();
                    groupedJeux.idType = g.Select(l => l.idType.First()).Distinct().ToList();
                    groupedJeux.idTheme = g.Select(l => l.idTheme.First()).Distinct().ToList();
                    return groupedJeux;
                }).First();

                edit = getTypesThemes(edit);
                return View(edit);
            }
        }
        [HttpPost]
        public IActionResult Editer([FromRoute] int id, [FromForm] EditeurJeuViewModel jeu)
        {
            if (id != jeu.Jeu.jeuid)
            {
                return BadRequest();
            }
            try
            {
                // vérification de la validité du model(livre)
                if (!ModelState.IsValid)
                {
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }
                // gestion de la couverture si une image est fournie
                if (jeu.Jeu.imageFile != null && jeu.Jeu.imageFile.Length > 0)
                {
                    if (System.IO.File.Exists("wwwroot/img/jeu/" + jeu.Jeu.image))
                    {
                        System.IO.File.Delete("wwwroot/img/jeu/" + jeu.Jeu.image);
                    }
                    jeu.Jeu.image = ManageCover(jeu.Jeu.imageFile!);
                }
                string queryJeux = "UPDATE jeux SET titre=@titre, description=@description, image=@image, nombrejoueursrecommandes=@nombrejoueursrecommandes, tempsjeumoyen=@tempsjeumoyen WHERE id=@id";
                string queryRemoveTypes = "DELETE FROM jeux_type WHERE jeuid=@id";
                string queryRemoveThemes = "DELETE FROM jeux_theme WHERE jeuid=@id";
                string queryTypes = "INSERT INTO jeux_type (jeuid, typeid) VALUES (@id,@typeid)";
                string queryThemes = "INSERT INTO jeux_theme (jeuid, themeid) VALUES (@id,@themeid)";

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        // modification jeux 
                        int res = connexion.Execute(queryJeux, jeu);
                        if (res != 1)
                        {
                            transaction.Rollback();
                            throw new Exception("Erreur pendant la mise à jour du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                        }
                        else
                        {
                            // suppression des anciennes catégories 
                            connexion.Execute(queryRemoveTypes, new { id = id });
                            connexion.Execute(queryRemoveThemes, new { id = id });

                            List<object> listTheme = new List<object>();
                            foreach (int theme_id in jeu.SelectedTypes)
                            {
                                listTheme.Add(new { jeuid = id, themeid = theme_id });
                            }
                            res = connexion.Execute(queryThemes, listTheme);
                            if (res != jeu.Jeu.idTheme.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeux. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }

                            List<object> listType = new List<object>();
                            foreach (int type_id in jeu.SelectedThemes)
                            {
                                listTheme.Add(new { jeuid = id, themeid = type_id });
                            }
                            res = connexion.Execute(queryThemes, listType);
                            if (res != jeu.Jeu.idType.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeux. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }

                            transaction.Commit();

                        }
            catch (Exception e) {
                // suppresion de la couverture dans le système de fichier si il y en a une
                if (jeu.Jeu.imageFile != null && System.IO.File.Exists("wwwroot" + jeu.Jeu.image))
                {
                    System.IO.File.Delete("wwwroot" + jeu.Jeu.image);
                }
                EditeurJeuViewModel jeuViewModel = new() { action = "Editer", titre = "Modification livre", idJeu = id };
                jeuViewModel.Jeu = jeu.Jeu;
                jeuViewModel.categories = GetCategories(null, livre.categoriesIDs);
                ViewData["ValidateMessage"] = e.Message;
                return View("Editeur", livreViewModel);

            }
            
            }

        }

}
