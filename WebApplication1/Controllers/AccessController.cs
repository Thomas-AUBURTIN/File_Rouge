using System.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Npgsql;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using BC = BCrypt.Net.BCrypt;

namespace WebApplication1.Controllers
{
    // Contrôleur principal pour la gestion des erreurs globales du site
    public class AccessController : Controller
    {

        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public AccessController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionBibliotheque")!;
            // si la chaîne de connexion n'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // Action affichant une liste de jeux spécifiques (id 1 à 4)
        public IActionResult Index()
        {

            ConnectionViewModel connection = new ConnectionViewModel();


            connection.ListJeux = ListeJeux();

            // Retourne la vue avec la liste des jeux
            return View(connection);



        }


        [HttpPost]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            // Vérifie la validité du modèle
            if (!ModelState.IsValid)
            {
                throw new Exception("Les données rentrées ne sont pas correctes, veuillez réessayer.");
            }
            string querysecure = "select * from UTILISATEURS U where pseudo=@pseudo or nom = @nom or telephone = @tel;";
            string query = "INSERT INTO Utilisateurs (nom,email,MOTDEPASSE,telephone,verificationtoken,administrateur,pseudo,dateinscription,emailverified) VALUES (@nom,@email,@mdp,@tel,@veriftoken,false,@pseudo,@date,false)";



            using (var transaction = new NpgsqlConnection(_connexionString))
            {
                int secure = transaction.ExecuteScalar<int>(querysecure, new
                {
                    nom = utilisateur.nom,
                    tel = utilisateur.Telephone,
                    pseudo = utilisateur.pseudo,

                });
                if (secure > 0)
                {
                    ViewData["ValidateMessage"] = " Utilisateur deja connue";

                    return RedirectToAction("Index", "Access");

                }



                // hachage du mot de passe
                string motDePasseHache = BC.HashPassword(utilisateur.motdePasse);
                // génération du token de vérification d'adresse mail
                byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                byte[] key = Guid.NewGuid().ToByteArray();
                string token = Convert.ToBase64String(time.Concat(key).ToArray());
                try
                {
                    using (var connexion = new NpgsqlConnection(_connexionString))
                    {
                        int res = connexion.Execute(query, new
                        {
                            nom = utilisateur.nom,
                            email = utilisateur.email,
                            tel = utilisateur.Telephone,
                            mdp = motDePasseHache,
                            veriftoken = token,
                            pseudo = utilisateur.pseudo,
                            date = DateTime.UtcNow

                        });

                        if (res != 1)
                        {
                            throw new Exception("Erreur pendant l'inscription, essai plus tard.");
                        }
                        else
                        {
                            UriBuilder uriBuilder = new UriBuilder();
                            uriBuilder.Port = 5248;
                            uriBuilder.Path = "/Access/Verifyemailpage";
                            uriBuilder.Query = $"email={HttpUtility.UrlEncode(utilisateur.email)}&token={HttpUtility.UrlEncode(token)}";

                            // envoi du mail avec le token
                            MailMessage mail = new MailMessage();
                            mail.From = new MailAddress("app@test.fr");
                            mail.To.Add(new MailAddress(utilisateur.email));
                            mail.Subject = "Vérification d'email";
                            mail.Body = $"<a href={uriBuilder.Uri}>Vérifier l'email</a>";
                            mail.IsBodyHtml = true; // permet de dire que le corps du message contient de l'html afin que le client mail affiche le corps du message en html (comme un navigateur)

                            using (var smtp = new SmtpClient("localhost", 587))
                            {
                                smtp.Credentials = new NetworkCredential("app@test.fr", "123456");
                                smtp.EnableSsl = false; // devrait être à true mais l'environnement de test ne le permet pas
                                smtp.Send(mail);
                            }

                            return RedirectToAction("ResultatInscription", "Access");
                        }
                    }
                }
                catch (Exception e)
                {
                    ViewData["ValidateMessage"] = e.Message;  // TODO à ajouter dans la vue
                    return RedirectToAction("SignUp", "Access");
                }
            }
        }

        public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            // TODO vérifier qu'on recoit bien des truc
            string query = "UPDATE Utilisateurs SET emailverified=true WHERE email=@email AND verificationtoken=@token";
            try
            {


                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new { email = email, token = token });
                    if (res != 1)
                    {
                        throw new Exception("Pb pendant la vérif, veuillez recommencer");
                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Email vérifié, vous pouvez maintenant vous connecter.";
                        return RedirectToAction("Index", "Verifyemailpage");
                    }
                }
            }

            catch (Exception e)
            {
                ViewData["ValidateMessage"] = e.Message;
                return RedirectToAction("Index", "Access");

            }
        }
        [HttpGet]
        public IActionResult SignUp()
        {
            return View("inscription");
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {


            // TODO enlever l'étoile
            string query = "SELECT * FROM Utilisateurs WHERE pseudo=@pseudo AND emailverified=true";
            try
            {
                Utilisateur userFromBDD;
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    List<Utilisateur> users = connexion.Query<Utilisateur>(query,

                    new { pseudo = utilisateur.pseudo }

                    ).ToList();
                    if (users.Count != 1)
                    {

                        // TODO gérer les erreurs du model et vider mot de passe
                        return RedirectToAction("Index", "Access");
                    }
                    userFromBDD = users.First();
                }

                userFromBDD.email = "user1@test.fr";

                // vérifier le mot de passe
                if (BC.Verify(utilisateur.motdePasse, userFromBDD.motdePasse))
                {
                    // création des revendications de l'utilisateur
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Email, userFromBDD.email),
                        new Claim(ClaimTypes.NameIdentifier,userFromBDD.utilisateurId.ToString()),
                        new Claim(ClaimTypes.Name, userFromBDD.nom),

                    };

                    List<string> roles = new List<string> { "User" }; // Tous les utilisateurs ont le rôle "User" par défaut

                    if (userFromBDD.administrateur == true)
                    {
                        roles.Add("Admin"); // Ajouter le rôle "Admin" si l'utilisateur est un administrateur
                    }

                    // Créer une revendication de rôle pour chaque rôle dans la liste
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // création du cookie
                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                    };

                    // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    if (Request.Form.ContainsKey("ReturnUrl"))
                    {
                        return Redirect(Request.Form["ReturnUrl"]!);
                    }
                    return RedirectToAction("Index", "Acceuil");
                }
                else
                {
                    // TODO gérer les erreurs du model et vider mot de passe
                    return RedirectToAction("Index", "Access");
                }
            }
            catch (Exception e)
            {
                // TODO a faire
                return RedirectToAction("Index", "Access");
            }
        }

        public async Task<IActionResult> SignOut()
        {
            // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Access");
        }

        

        public List<Jeux> ListeJeux()
        {
            //Requête SQL pour récupérer certains jeux
            string query = "SELECT * FROM JEUX WHERE AGE(now(), DATEAJOUT) < INTERVAL '1 month';";
            List<Jeux> ListJeux = new List<Jeux>();
            try
            {
                // Connexion à la base et exécution de la requête
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    ListJeux = connexion.Query<Jeux>(query).ToList();
                }
                // Retourne la vue avec la liste des jeux
                if (ListJeux.Count() < 2)
                {
                    string query4 = "SELECT * FROM JEUX ORDER BY DATEAJOUT DESC LIMIT 4;";
                    List<Jeux> derniersJeux = new List<Jeux>();
                    try
                    {
                        // Connexion à la base et exécution de la requête
                        using (var connexion = new NpgsqlConnection(_connexionString))
                        {
                            ListJeux = connexion.Query<Jeux>(query4).ToList();
                        }
                    }
                    catch
                    {
                        // En cas d'erreur, retourne une liste vide
                        return new List<Jeux>();
                    }



                }
                return ListJeux;
            }
            catch
            {
                // En cas d'erreur, retourne une page NotFound
                return null;
            }
        }
        public IActionResult ResultatInscription()
        {
            return View("ResultatInscription");
        }
        public IActionResult Verifyemailpage()
        {
            return View("Verifyemailpage");
        }

        public IActionResult Profil()
        {
            ProfilViewModel profil = new ProfilViewModel();

            string query = "SELECT * FROM Utilisateurs WHERE utilisateurid=@id";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                profil.utilisateur = connexion.QuerySingle<Utilisateur>(query, new { id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) });
            }
            string querycom = "SELECT C.*, J.titre AS jeuNom " +
                "FROM Commentaires C  " +
                "JOIN Jeux J ON C.jeuId = J.jeuid " +
                "WHERE C.utilisateurid=@id";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                profil.Commentaires = connexion.Query<Commentaire>(querycom, new { id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) }).ToList();
            }


            TempData["profil"] = "profil";
            return View("Profil",profil);
        }

    }

}
