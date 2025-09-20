using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace WebApplication1.Controllers
{
    // Contrôleur pour la gestion du catalogue de jeux
    [Authorize(Roles = "User")]

    public class CatalogueController : Controller
    {
        // Chaîne de connexion à la base de données
        private readonly string _connexionString;

        // Constructeur : récupère la chaîne de connexion depuis la configuration
        public CatalogueController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // Affiche la liste des jeux, triés par titre
        public IActionResult Index(int i, string sort)
        {
            string query = "select J.* from jeux J order by J.TITRE";
            CatalogueViewModel catalogue_jeux = new CatalogueViewModel();
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    catalogue_jeux.ListJeux = connexion.Query<Jeux>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            catalogue_jeux = getTypesThemes(catalogue_jeux);
            ViewData["i"] = i;
            return View(catalogue_jeux);
        }


        // Affiche la page d'erreur
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Trie les jeux selon les filtres sélectionnés (thème, type, nombre de joueurs, temps)
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Trie([FromForm] CatalogueViewModel typejeux)
        {
            CatalogueViewModel Jeutrie = new CatalogueViewModel();

            // Construction dynamique de la requête SQL selon les filtres
            string querytrie = "select distinct j.*   from JEUX j  join JEUX_THEME jt1 on j.JEUID = jt1.JEUID  join THEME T1 on t1.THEMEID  = jt1.THEMEID  join JEUX_type jt2 on j.JEUID = jt2.JEUID  join Types T2 on t2.TYPEID = jt2.TYPEID  where";
            bool first = true;
            if (typejeux.SelectedTheme != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " t1.themeid = @theme ";
            }
            if (typejeux.SelectedType != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " t2.typeid = @type ";
            }
            if (typejeux.SelectedJoueur != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " j.nombrejoueursrecommandes = @joueur ";
            }
            if (typejeux.Selectedtemps != null)
            {
                if (first) { first = false; }
                else { querytrie += " and "; }
                querytrie += " j.tempsjeumoyen = @temps ";
            }
            querytrie += "  order by j.titre; ";

            // Exécution de la requête avec gestion des exceptions
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
            Jeutrie = getTypesThemes(Jeutrie);
            ViewData["i"] = 0;
            return View("Index", Jeutrie);
        }

        // Récupère les listes de thèmes, types, joueurs et temps pour les filtres du catalogue
        [ValidateAntiForgeryToken]
        public CatalogueViewModel getTypesThemes(CatalogueViewModel listjeux)
        {
            // Ajoute les options par défaut
            listjeux.ListThemes.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListTypes.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListJoueur.Add(new SelectListItem("--Choix tu trie--", ""));
            listjeux.ListTemps.Add(new SelectListItem("--Choix tu trie--", ""));

            // Récupère les thèmes disponibles
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                string queryThemes = "select distinct j.themeid , t.nom from jeux_theme j   join THEME t on j.themeid=t.themeid order by themeid ";
                List<Theme> Themes = connexion.Query<Theme>(queryThemes).ToList();
                foreach (Theme Theme in Themes)
                {
                    listjeux.ListThemes.Add(new SelectListItem(Theme.nom, Theme.themeId.ToString()));
                }
            }

            // Récupère les types disponibles
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                string queryTypes = "select distinct j.TYPEID  , t.nom from JEUX_TYPE  j   join types t on j.typeid=t.typeid order by typeid ";
                List<JeuxType> Types = connexion.Query<JeuxType>(queryTypes).ToList();
                foreach (JeuxType type in Types)
                {
                    listjeux.ListTypes.Add(new SelectListItem(type.nom, type.typeId.ToString()));
                }
            }

            // Récupère les temps de jeu disponibles
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                string queryTemps = "select distinct tempsjeumoyen from JEUX order by TEMPSJEUMOYEN  ";
                List<int> Temps = connexion.Query<int>(queryTemps).ToList();
                foreach (int temp in Temps)
                {
                    listjeux.ListTemps.Add(new SelectListItem(temp.ToString(), temp.ToString()));
                }
            }

            // Récupère les nombres de joueurs disponibles
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

        // Affiche le détail d'un jeu (avec ses thèmes, types et commentaires)

        public IActionResult Detail(int id)
        {
            Jeux jeu;
            string queryjeux = "select j.*,t1.*,t2.* ,c.* from jeux j " +
                "left join JEUX_THEME JT1 on j.JEUID = jt1.JEUID " +
                "left join theme t1 on t1.THEMEID  = jt1.THEMEID " +
                "left join JEUX_TYPE JT2 on j.JEUID = jt2.JEUID " +
                "left join types t2 on t2.typeID  = jt2.typeID " +
                "left join COMMENTAIRES C  on c.JEUID = j.JEUID  " +
                "where j.JEUID = @id";
            List<Jeux> jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    // Mapping des objets liés (thème, type, commentaire)
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

                // Regroupe les données pour éviter les doublons
                jeu = jeux.GroupBy(j => j.jeuid).Select(g =>
                {
                    Jeux groupedJeu = g.First();
                    groupedJeu.commentaires = g
                        .SelectMany(j => j.commentaires)
                        .Where(c => c != null)
                        .DistinctBy(c => new { c.jeuId, c.utilisateurId, c.commentaire, c.datecommentaires })
                        .ToList();

                    if (groupedJeu.types.Count != 0)
                    {
                        groupedJeu.types = g.Select(j => j.types.Single())
                                                    .DistinctBy((c) => c.typeId)
                                                    .ToList();
                    }
                    if (groupedJeu.themes.Count != 0)
                    {
                        groupedJeu.themes = g.Select(j => j.themes.Single())
                                                    .DistinctBy((c) => c.themeId)
                                                    .ToList();
                    }
                    return groupedJeu;
                }).First();

                string querycommentaires = "select * from COMMENTAIRES where JEUID = @jeuid and UTILISATEURID = @utilid;";
                var user = HttpContext.User;
                string? userId = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int utilid = int.Parse(userId);

                using (var connexioncommentaire = new NpgsqlConnection(_connexionString))
                {
                    var commentaire = connexioncommentaire.QueryFirstOrDefault(querycommentaires, new
                    {
                        jeuid = id,
                        utilid = utilid


                    });
                    if (commentaire == null)
                    {
                        ViewData["comm"] = true;

                        return View(jeu);
                    }
                    else
                    {
                        ViewData["comm"] = false;
                        return View(jeu);
                    }

                }
            }
        }

        // Récupère les listes de thèmes, types, joueurs et temps pour l'édition d'un jeu
        [ValidateAntiForgeryToken]
        public EditeurJeuViewModel getTypesThemes(EditeurJeuViewModel singleJeux)
        {
            // Ajoute les options par défaut
            singleJeux.ListThemes.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListTypes.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListJoueur.Add(new SelectListItem("--Choix tu trie--", ""));
            singleJeux.ListTemps.Add(new SelectListItem("--Choix tu trie--", ""));

            // Récupère les thèmes et coche ceux du jeu en cours d'édition
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

            // Récupère les types et coche ceux du jeu en cours d'édition
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

            // Récupère les temps de jeu et coche celui du jeu en cours d'édition
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

            // Récupère les nombres de joueurs et coche celui du jeu en cours d'édition
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

        // Gère l'enregistrement de l'image de couverture d'un jeu
        private string ManageCover(IFormFile file)
        {
            string[] permittedExtensions = { ".jpeg", ".jpg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Vérifie l'extension du fichier
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                ModelState["jeu.imageFile"]!.Errors.Add(new ModelError("Ce type de fichier n'est pas accepté."));
                throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
            }

            // Génère un chemin de fichier unique
            string? filePath = Path.Combine("/img/jeu/",
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName)).ToString();

            // Sauvegarde le fichier sur le serveur
            using (var stream = System.IO.File.Create("wwwroot" + filePath))
            {
                file.CopyTo(stream);
            }
            return filePath;
        }

        // Affiche la page d'édition d'un jeu (GET)
        [HttpGet]
        [Authorize(Roles = "Admin")]

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
                    // Mapping des objets liés
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

                // Regroupe les données pour l'affichage
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
                ViewData["titre"] = "Editer";
                return View(edit);
            }
        }

        // Traite la soumission du formulaire d'édition d'un jeu (POST)
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public IActionResult Editer([FromQuery] int id, [FromForm] Jeux jeu)
        {
            if (id != jeu.jeuid)
            {
                return BadRequest();
            }
            try
            {
                // Vérifie la validité du modèle
                if (!ModelState.IsValid)
                {
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }

                // Gère la mise à jour de l'image si besoin
                if (jeu.imageFile != null && jeu.imageFile.Length > 0)
                {
                    if (System.IO.File.Exists("wwwroot/img/jeu/" + jeu.image))
                    {
                        System.IO.File.Delete("wwwroot/img/jeu/" + jeu.image);
                    }
                    jeu.image = ManageCover(jeu.imageFile!);
                }

                // Requêtes SQL pour la mise à jour du jeu et de ses liens
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
                        // Met à jour le jeu
                        int res = connexion.Execute(queryJeux, jeu);
                        if (res != 1)
                        {
                            transaction.Rollback();
                            throw new Exception("Erreur pendant la mise à jour du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                        }
                        else
                        {
                            // Supprime les anciens liens types/thèmes
                            connexion.Execute(queryRemoveTypes, new { id = id });
                            connexion.Execute(queryRemoveThemes, new { id = id });

                            // Ajoute les nouveaux liens thèmes
                            List<object> listTheme = new List<object>();
                            foreach (int theme_id in jeu.idThemes)
                            {
                                listTheme.Add(new { jeuid = id, themeid = theme_id });
                            }
                            res = connexion.Execute(queryThemes, listTheme);
                            if (res != jeu.idThemes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                            }

                            // Ajoute les nouveaux liens types
                            List<object> listType = new List<object>();
                            foreach (int type_id in jeu.idTypes)
                            {
                                listType.Add(new { jeuid = id, typeid = type_id });
                            }
                            res = connexion.Execute(queryTypes, listType);
                            if (res != jeu.idTypes.Count)
                            {
                                transaction.Rollback();
                                throw new Exception("Erreur pendant la mise à jour du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
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
                // Nettoie l'image si erreur
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

        // Affiche la page de création d'un nouveau jeu
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Nouveau()
        {
            EditeurJeuViewModel jeuViewModel = new() { action = "Nouveau", titre = "Nouveau Livre" };
            jeuViewModel = getTypesThemes(jeuViewModel);
            jeuViewModel.Jeu = new Jeux();
            ViewData["titre"] = "Nouveau";
            return View("Editer", jeuViewModel);
        }

        // Traite la soumission du formulaire de création d'un nouveau jeu
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Nouveau([FromForm] Jeux jeu)
        {
            try
            {
                // Vérifie la validité du modèle
                if (!ModelState.IsValid)
                {
                    throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                }

                // Vérifie l'unicité du titre
                string queryTitre = "select j.* from JEUX j where j.titre = @titre;";
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    if (connexion.Query(queryTitre, new { titre = jeu.titre }).Count() > 0)
                    {
                        ModelState["jeu.titre"]!.Errors.Add(new ModelError("Ce titre existe déjà."));
                        throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
                    }
                }

                // Gère l'image si présente
                if (jeu.imageFile != null && jeu.imageFile.Length > 0)
                {
                    jeu.image = ManageCover(jeu.imageFile!);
                }

                jeu.dateajout = DateTime.Now;

                // Requêtes SQL pour l'insertion du jeu et de ses liens
                string queryJeu = "insert into jeux (TITRE, DESCRIPTION, image,NOMBREJOUEURSRECOMMANDES, TEMPSJEUMOYEN,DATEAJOUT) values (@titre, @description, @image, @nombrejoueursrecommandes, @tempsjeumoyen, @dateajout) RETURNING jeuid;";
                string queryTypes = "INSERT INTO jeux_type (jeuid, typeid) VALUES (@jeuid,@typeid)";
                string queryThemes = "INSERT INTO jeux_theme (jeuid, themeid) VALUES (@jeuid,@themeid)";

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        // Insère le jeu
                        int jeu_id = connexion.ExecuteScalar<int>(queryJeu, jeu);
                        if (jeu_id == 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Erreur pendant la création du jeu. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                        }
                        else
                        {
                            // Insère les liens thèmes
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

                            // Insère les liens types
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
                // Nettoie l'image si erreur
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

        // Supprime un jeu et ses liens (types, thèmes, commentaires)
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public IActionResult Supprimer([FromQuery] int id, [FromForm] int idJeu)
        {
            if (id != idJeu)
            {
                return BadRequest();
            }

            // Requêtes SQL pour la suppression
            string querySupprimerLiensTypes = "DELETE FROM jeux_type WHERE jeuid=@id;";
            string querySupprimerLiensThemes = "DELETE FROM jeux_theme WHERE jeuid=@id;";
            string queryUpdateCommentaire = "UPDATE commentaires SET  jeuid=null WHERE jeuid=@id";
            string querySupprimerJeux = "DELETE FROM jeux WHERE jeuid=@id;";

            string queryNombresThemes = "SELECT count(*) FROM jeux_theme WHERE jeuid=@id";
            string queryNombresTypes = "SELECT count(*) FROM jeux_type WHERE jeuid=@id";
            string queryNombresCommentaires = "SELECT count(*) FROM commentaires WHERE jeuid=@id";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        // Supprime les liens thèmes
                        int nbTheme = connexion.ExecuteScalar<int>(queryNombresThemes, new { id = id });
                        int res = connexion.Execute(querySupprimerLiensThemes, new { id = id });
                        if (res != nbTheme)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }

                        // Supprime les liens types
                        int nbType = connexion.ExecuteScalar<int>(queryNombresTypes, new { id = id });
                        res = connexion.Execute(querySupprimerLiensTypes, new { id = id });
                        if (res != nbType)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }

                        // Met à jour les commentaires liés
                        int nbCommentaires = connexion.ExecuteScalar<int>(queryNombresCommentaires, new { id = id });
                        res = connexion.Execute(queryUpdateCommentaire, new { id = id });
                        if (res != nbCommentaires)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }

                        // Supprime l'image du jeu si présente
                        string queryimage = "SELECT image from Jeux where jeuid=@id";
                        string image = connexion.ExecuteScalar<string>(queryimage, new { id = id });
                        res = connexion.Execute(querySupprimerJeux, new { id = id });
                        if (res != 1)
                        {
                            transaction.Rollback();
                            throw new Exception("Problème pendant la suppression. Veuillez réessayer plus tard. ");
                        }
                        if (image is not null && System.IO.File.Exists("wwwroot" + image))
                        {
                            System.IO.File.Delete("wwwroot" + image);
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
        [HttpGet]
        public IActionResult FaireCommentaire(int id)
        {
            EditeurCommentaireViewModel editer = new() { Action = "EnvoyerCommentaire", Titre = "Modification jeu", Commentaire = new Commentaire() };

            ViewData["jeuid"] = id;


            // Récupération de l'ID de l'utilisateur à partir du token
            var user = HttpContext.User;
            string? userId = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewData["utilisateurid"] = int.Parse(userId);



            return View("commentaire", editer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Ajouté
        public IActionResult EnvoyerCommentaire([FromForm] Commentaire Commentaire)
        {
            string querycheck = "SELECT nom from utilisateurs where utilisateurid=@utilisateurid;";

            using (var connexioncheck = new NpgsqlConnection(_connexionString))
            {
                var nom = connexioncheck.ExecuteScalar<string>(querycheck, new { utilisateurid = Commentaire.utilisateurId });
                Commentaire.utilisateurNom = nom;
            }

            string query = "insert into commentaires (jeuId, utilisateurId, commentaire, datecommentaires,note,utilisateurnom) values (@jeuId, @utilisateurId, @commentaire, @datecommentaires,@note,@nom);";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    int res = connexion.Execute(query, new
                    {
                        jeuId = Commentaire.jeuId,
                        utilisateurId = Commentaire.utilisateurId,
                        commentaire = Commentaire.commentaire,
                        datecommentaires = DateTime.UtcNow,
                        note = Commentaire.note,
                        nom = Commentaire.utilisateurNom

                    });
                    if (res != 1)
                    {
                        throw new Exception("Erreur pendant l'ajout du commentaire. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                    }
                }
                catch (Exception e)
                {
                    ViewData["ValidateMessage"] = e.Message;
                    return View();
                }
                return RedirectToAction("Detail", new { id = Commentaire.jeuId });

            }
        }
        [HttpGet]
        public IActionResult Recherche([FromQuery] String nom)
        {

            string query = "SELECT * FROM Jeux WHERE lower(titre) like lower(@titre)";
            List<Jeux> Jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Jeux = connexion.Query<Jeux>(query, new { titre = "%" + nom + "%" }).ToList();
            }
            return Json(Jeux);


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SupprimerCommentaire([FromRoute] int id, [FromRoute] int jeuid)
        {
            string querySupprimercomm = "DELETE FROM commentaires WHERE jeuid=@jeuid and utilisateurid=@id;";
            string queryNombresThemes = "SELECT count(*) FROM commentaires WHERE jeuid=@jeuid and utilisateurid=@id;";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var transaction = connexion.BeginTransaction())
                {
                    // Supprime les liens thèmes
                    int nbcomm = connexion.ExecuteScalar<int>(queryNombresThemes, new { id = id, jeuid = jeuid });
                    int res = connexion.Execute(querySupprimercomm = "DELETE FROM commentaires WHERE jeuid=@jeuid and utilisateurid=@id;"
, new { id = id });
                    if (res != nbcomm)
                    {
                        transaction.Rollback();

                        return RedirectToAction("Detail", jeuid);
                    }
                    return RedirectToAction("Detail", jeuid);


                }
            }
        }

        [HttpGet]
        public IActionResult EditerCommentaire([FromQuery] int id, [FromQuery] int jeuid)
        {
            Commentaire commentaire = null;
            EditeurCommentaireViewModel edit = new() { Action = "EditerCommentaire", Titre = "Modification Commentaire" };

            string query = "SELECT * FROM commentaires WHERE jeuid = @jeuid AND utilisateurid = @id";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    commentaire = connexion.QueryFirstOrDefault<Commentaire>(query, new { jeuid = jeuid, id = id });

                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Erreur lors de la récupération du commentaire.");
                }
            }

            edit.Commentaire = commentaire;
            return View("Commentaire", edit);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult EditerCommentaire([FromForm] Commentaire Commentaire)
        {


            string query = "UPDATE commentaires SET commentaire=@commentaire, note=@note, datecommentaires=@datecommentaires WHERE jeuid=@jeuid AND utilisateurid=@utilisateurid";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    int res = connexion.Execute(query, new
                    {
                        commentaire = Commentaire.commentaire,
                        note = Commentaire.note,
                        datecommentaires = DateTime.UtcNow,
                        jeuid = Commentaire.jeuId,
                        utilisateurid = Commentaire.utilisateurId
                    });
                    if (res != 1)
                    {
                        throw new Exception("Erreur pendant la mise à jour du commentaire. Veuillez réessayer plus tard. Si le problème persiste merci de contacter l'administrateur.");
                    }
                }
                catch (Exception e)
                {
                    ViewData["ValidateMessage"] = e.Message;
                    return View();
                }



                return RedirectToAction("VerifCommentaire", "Catalogue");

            }

        }
        public IActionResult VerifCommentaire()
        {
            return View("VerifCommentaire");
        }
    }
}