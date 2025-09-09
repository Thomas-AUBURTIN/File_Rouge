
using System;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc; // Pour les contrôleurs MVC
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    // Contrôleur pour la gestion des pages de pied de page (footer)
    public class PiedsController : Controller
    {
        private readonly string _connexionString;
        public PiedsController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("GestionBibliotheque");
        }


        // Action pour afficher la page d'accueil du pied de page
        public IActionResult Index()
        {
            return View(); // Retourne la vue associée à Index
        }

        // Action pour afficher la page de confidentialité
        public IActionResult Privacy()
        {
            return View(); // Retourne la vue associée à Privacy
        }

        // Action pour afficher la page de contact
        [HttpGet]
        public IActionResult Contact()
        {
            return View(); // Retourne la vue associée à Contact
        }
        [HttpPost]
        public IActionResult Mails([FromForm] ContactViewModel contact)
        {
            if (contact.nom == null && contact.email == null)
            {

                int userId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                string query = "SELECT * FROM Utilisateurs WHERE utilisateurId = @Id";
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    Utilisateur util = connexion.Query<Utilisateur>(query, new
                    {
                        Id = userId

                    }).ToList().SingleOrDefault();




                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(util.email);
                    mail.To.Add(new MailAddress("app@test.fr"));
                    mail.Subject = contact.Sujet;
                    mail.Body = contact.texte;
                    mail.IsBodyHtml = true;
                    using (var smtp = new SmtpClient("localhost", 587))
                    {
                        smtp.Credentials = new NetworkCredential(util.email, "123456");
                        smtp.EnableSsl = false; // devrait être à true mais l'environnement de test ne le permet pas
                        smtp.Send(mail);
                    }
                    return RedirectToAction("Index", "Acceuil");

                }

            }
            else if (contact.nom != null && contact.email != null)
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("guest@test.fr");//mail test pour inviter
                mail.To.Add(new MailAddress("app@test.fr"));
                mail.Subject = contact.Sujet;
                mail.Body = contact.texte;
                mail.IsBodyHtml = true;
                using (var smtp = new SmtpClient("localhost", 587))
                {
                    smtp.Credentials = new NetworkCredential("guest@test.fr", "123456");
                    smtp.EnableSsl = false; // devrait être à true mais l'environnement de test ne le permet pas
                    smtp.Send(mail);
                }
                return RedirectToAction("Index", "Access");
            }
            else
            {
                //todo mettre un message
                return RedirectToAction("Contact", "Pieds");
            }

        }
    }
}