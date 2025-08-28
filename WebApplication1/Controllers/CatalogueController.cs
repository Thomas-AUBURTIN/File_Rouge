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
                        bool selected = false;
                        if (singleJeux.Jeu != null)
                        {
                            selected = singleJeux.Jeu.idThemes.Contains(Theme.themeId);
                        }
                        singleJeux.ListThemes.Add(new SelectListItem(Theme.nom, Theme.themeId.ToString(), selected));
                    }

                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTypes = "select distinct j.TYPEID  , t.nom from JEUX_TYPE  j   join types t on j.typeid=t.typeid order by typeid ";
                    List<JeuxType> Types = connexion.Query<JeuxType>(queryTypes).ToList();
                    foreach (JeuxType type in Types)
                    {
                        bool selected = false;
                        if (singleJeux.Jeu != null)
                        {
                            selected = singleJeux.Jeu.idTypes.Contains(type.typeId) ? true : false;
                        }
                        singleJeux.ListTypes.Add(new SelectListItem(type.nom, type.typeId.ToString(), selected));
                    }


                }

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryTemps = "select distinct tempsjeumoyen from JEUX order by TEMPSJEUMOYEN  ";
                    List<int> Temps = connexion.Query<int>(queryTemps).ToList();
                    foreach (int temp in Temps)
                    {
                        bool selected = false;
                        if (singleJeux.Jeu != null)
                        {
                            selected = singleJeux.Jeu.tempsjeumoyen == temp ? true : false;
                        }
                        singleJeux.ListTemps.Add(new SelectListItem(temp.ToString(), temp.ToString(), selected));
                    }


                }
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string queryJoueurs = "select distinct NOMBREJOUEURSRECOMMANDES  from JEUX order by NOMBREJOUEURSRECOMMANDES   ";
                    List<int> Joueurs = connexion.Query<int>(queryJoueurs).ToList();
                    foreach (int Joueur in Joueurs)
                    {
                        bool selected = false;
                        if (singleJeux.Jeu != null)
                        {
                            selected = singleJeux.Jeu.nombrejoueursrecommandes == Joueur ? true : false;
                        }
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
        public IActionResult Editer(int id)
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
                        jeu.idTypes.Add(type.typeId);
                        jeu.idThemes.Add(theme.themeId);

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
                    groupedJeux.idTypes = g.Select(l => l.idTypes.First()).Distinct().ToList();
                    groupedJeux.idThemes = g.Select(l => l.idThemes.First()).Distinct().ToList();
                    return groupedJeux;
                }).First();

                edit = getTypesThemes(edit);
                return View(edit);
            }
        }
        [HttpPost]
        public IActionResult Editer([FromRoute] int id, [FromForm] Jeux jeu)
        {
            if (id != jeu.jeuid)
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
                if (jeu.imageFile != null && jeu.imageFile.Length > 0)
                {
                    if (System.IO.File.Exists("wwwroot/img/jeu/" + jeu.image))
                    {
                        System.IO.File.Delete("wwwroot/img/jeu/" + jeu.image);
                    }
                    jeu.image = ManageCover(jeu.imageFile!);
                }
                string queryJeux = "UPDATE jeux SET titre=@titre, description=@description, image=@image, nombrejoueursrecommandes=@nombrejoueursrecommandes, tempsjeumoyen=@tempsjeumoyen WHERE jeuid=@jeuid";
                string queryRemoveTypes = "DELETE FROM jeux_type WHERE jeuid=@id";
                string queryRemoveThemes = "DELETE FROM jeux_theme WHERE jeuid=@id";
                string queryTypes = "INSERT INTO jeux_type (jeuid, typeid) VALUES (@jeuid,@typeid)";
                string queryThemes = "INSERT INTO jeux_theme (jeuid, themeid) VALUES (@jeuid,@themeid)";

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
                            foreach (int theme_id in jeu.idThemes)
                            {
                                listTheme.Add(new { jeuid = id, themeid = theme_id });
                            }
                            res = connexion.Execute(queryThemes, listTheme);
                            if (res != jeu.idThemes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeux. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }

                            List<object> listType = new List<object>();
                            foreach (int type_id in jeu.idTypes)
                            {
                                listType.Add(new { jeuid = id, typeid = type_id });
                            }
                            res = connexion.Execute(queryTypes, listType);
                            if (res != jeu.idTypes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeux. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }

                            transaction.Commit();

                        }
                    }
                }
                ViewData["ValidateMessage"] = "Jeux mis à jour";
                EditeurJeuViewModel jeuViewModel = new() { action = "Editer", titre = "Modification Livre", idJeu = id };
                jeuViewModel.Jeu = jeu;
                jeuViewModel = getTypesThemes(jeuViewModel);

                return View("Editer", jeuViewModel);

            }


            catch (Exception e)
            {
                // suppresion de la couverture dans le système de fichier si il y en a une
                if (jeu.imageFile != null && System.IO.File.Exists("wwwroot" + jeu.image))
                {
                    System.IO.File.Delete("wwwroot" + jeu.image);
                }
                EditeurJeuViewModel jeuViewModel = new() { action = "Editer", titre = "Modification livre", idJeu = id };
                jeuViewModel.Jeu = jeu;
                jeuViewModel = getTypesThemes(jeuViewModel);
                ViewData["ValidateMessage"] = e.Message;
                return View("Editer", jeuViewModel);

            }

        }
        [HttpGet]
        public IActionResult Nouveau()
        {
            EditeurJeuViewModel jeuViewModel = new() { action = "Nouveau", titre = "Nouveau Livre" };
            jeuViewModel = getTypesThemes(jeuViewModel);
            jeuViewModel.Jeu = new Jeux();
            return View("Editer", jeuViewModel);
        }


        [HttpPost]
        public IActionResult Nouveau([FromForm] Jeux jeu)
        {
            
            try
            {
                // vérification de la validité du model (livre)
                if (!ModelState.IsValid)
                {
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }

                // vérification de la non existance du titre dans la bdd
                string queryTitre = "select j.* from JEUX j where j.titre = @titre;";
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    if (connexion.Query(queryTitre, new { titre = jeu.titre }).Count() > 0)
                    {
                        ModelState["jeu.titre"]!.Errors.Add(new ModelError("Ce titre existe déjà."));
                        throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                    }
                }
                // gestion de la couverture si une image est fournie
                if (jeu.imageFile != null && jeu.imageFile.Length > 0)
                {
                    jeu.image = ManageCover(jeu.imageFile!);
                }

                jeu.dateajout = DateTime.Now;
                // enregistrement du livre en BDD
                string queryJeu = "insert into jeux (TITRE, DESCRIPTION, image,NOMBREJOUEURSRECOMMANDES, TEMPSJEUMOYEN,DATEAJOUT) values (@titre, @description, @image, @nombrejoueursrecommandes, @tempsjeumoyen, @dateajout) RETURNING jeuid;";
                string queryTypes = "INSERT INTO jeux_type (jeuid, typeid) VALUES (@jeuid,@typeid)";
                string queryThemes = "INSERT INTO jeux_theme (jeuid, themeid) VALUES (@jeuid,@themeid)";

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        // insert du livre et récupération de son id
                        int jeu_id = connexion.ExecuteScalar<int>(queryJeu, jeu);
                        if (jeu_id == 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Erreur pendant la création du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                        }
                        else
                        {
                            // ajout des associations avec les themes
                            List<object> list = new List<object>();
                            foreach (int theme_id in jeu.idThemes)
                            {
                                list.Add(new { jeuid = jeu_id, themeid = theme_id });
                            }
                            int res = connexion.Execute(queryThemes, list);
                            if (res != jeu.idThemes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la création du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }
                            // ajout des associations avec les catégories
                            list = new List<object>();
                            foreach (int type_id in jeu.idTypes)
                            {
                                list.Add(new { jeuid = jeu_id, typeid = type_id });
                            }
                            res = connexion.Execute(queryTypes, list);
                            if (res != jeu.idTypes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la création du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }
                            transaction.Commit();
                        }
                    }

                }
                ViewData["ValidateMessage"] = "Livre bien créé !";
                EditeurJeuViewModel jeuViewModel = new() { action = "Nouveau", titre = "Nouveau Livre" };
                jeuViewModel.Jeu = new Jeux();
                jeuViewModel = getTypesThemes(jeuViewModel);
                return View("Editer", jeuViewModel);
            }
            catch (Exception e)
            {
                // suppresion de la couverture dans le système de fichier si il y en a une
                if (jeu.imageFile != null && System.IO.File.Exists("wwwroot" + jeu.image))
                {
                    System.IO.File.Delete("wwwroot" + jeu.image);
                }
                EditeurJeuViewModel jeuViewModel = new() { action = "Editer", titre = "Modification livre" };
                jeuViewModel.Jeu = jeu;
                jeuViewModel = getTypesThemes(jeuViewModel);
                ViewData["ValidateMessage"] = e.Message;
                return View("Editer", jeuViewModel);



            }


        }
        [HttpPost]
        public IActionResult Supprimer([FromRoute] int id, [FromForm] int idJeu)
        {
            if (id != idJeu)
            {
                return BadRequest();
            }

            // requêtes SQL
            string querySupprimerLiensTypes = "DELETE FROM jeux_type WHERE jeuid=@id;";
            string querySupprimerLiensThemes = "DELETE FROM jeux_theme WHERE jeuid=@id;";
            string queryUpdateCommentaire = "UPDATE commentaires SET  jeuid=null WHERE jeuid=@id";
            string querySupprimerLivre = "DELETE FROM jeux WHERE jeuid=@id;";

            string queryNombresThemes = "SELECT count(*) FROM jeu_theme WHERE jeuid=@id";
            string queryNombresTypes = "SELECT count(*) FROM jeu_type WHERE jeuid=@id";
            string queryNombresCommentaires = "SELECT count(*) FROM commentaires WHERE jeuid=@id";

            try
            {


                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        // récupération du nombre de catégories du livre
                        int nbTheme = connexion.ExecuteScalar<int>(queryNombresThemes, new { id = id });
                        // exécution de la req de suppression des catégories
                        int res = connexion.Execute(querySupprimerLiensThemes, new { id = id });
                        if (res != nbTheme)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }
                        // récupération du nombre de catégories du livre
                        int nbType = connexion.ExecuteScalar<int>(queryNombresTypes, new { id = id });
                        // exécution de la req de suppression des catégories
                        res = connexion.Execute(querySupprimerLiensTypes, new { id = id });
                        if (res != nbType)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }

                        // récupération du nombre d'emprunts de ce livre
                        int nbCommentaires = connexion.ExecuteScalar<int>(queryNombresCommentaires, new { id = id });
                        // mise à jour des emprunts
                        res = connexion.Execute(queryUpdateCommentaire, new { id = id });
                        if (res != nbCommentaires)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }

                        // suppression du livre
                        res = connexion.Execute(querySupprimerLivre, new { id = id });
                        if (res != 1)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }
                        transaction.Commit();
                    }

                }
            }
            catch (Exception e)
            {
                TempData["ValidateMessage"] = e.Message;
                return RedirectToAction("Index");
            }

            TempData["ValidateMessage"] = "Suppression effectuée.";
            return RedirectToAction("Index");
        }
    }
}
