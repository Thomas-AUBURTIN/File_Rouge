using System;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        // Chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur : récupère la chaîne de connexion depuis la configuration
        /// </summary>
        /// <param name="configuration">Configuration de l'application</param>
        public PiedsController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("GestionBibliotheque");
        }

        /// <summary>
        /// Action pour afficher la page d'accueil du pied de page
        /// </summary>
        /// <returns>Vue Index</returns>
        public IActionResult Index()
        {
            // Retourne la vue associée à Index
            return View();
        }

        /// <summary>
        /// Action pour afficher la page de confidentialité
        /// </summary>
        /// <returns>Vue Privacy</returns>
        public IActionResult Privacy()
        {
            // Retourne la vue associée à Privacy
            return View();
        }

        /// <summary>
        /// Action pour afficher la page de contact
        /// </summary>
        /// <returns>Vue Contact</returns>
        [HttpGet]
        public IActionResult Contact()
        {
            // Retourne la vue associée à Contact
            return View();
        }

        /// <summary>
        /// Action pour gérer l'envoi d'emails via le formulaire de contact
        /// </summary>
        /// <param name="contact">Modèle contenant les informations du formulaire de contact</param>
        /// <returns>Redirection vers une autre action ou vue</returns>
        [HttpPost]
        public IActionResult Mails([FromForm] ContactViewModel contact)
        {
            try
            {
                // Si l'utilisateur est connecté (nom et email non fournis dans le formulaire)
                if (contact.nom == null && contact.email == null)
                {
                    // Récupère l'ID de l'utilisateur connecté
                    int userId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    string query = "SELECT * FROM Utilisateurs WHERE utilisateurId = @Id";

                    using (var connexion = new NpgsqlConnection(_connexionString))
                    {
                        connexion.Open();
                        // Récupère les informations de l'utilisateur depuis la base de données
                        Utilisateur util = connexion.Query<Utilisateur>(query, new { Id = userId }).SingleOrDefault();

                        // Préparation et envoi de l'email
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(util.email);
                        mail.To.Add(new MailAddress("app@test.fr"));
                        mail.Subject = contact.Sujet;
                        mail.Body = contact.texte;
                        mail.IsBodyHtml = true;

                        using (var smtp = new SmtpClient("localhost", 587))
                        {
                            smtp.Credentials = new NetworkCredential(util.email, "123456");
                            smtp.EnableSsl = false; // Devrait être à true en production
                            smtp.Send(mail);
                        }
                    }

                    // Redirige vers la page d'accueil après l'envoi
                    return RedirectToAction("Index", "Acceuil");
                }
                // Si l'utilisateur est un invité (nom et email fournis dans le formulaire)
                else if (contact.nom != null && contact.email != null)
                {
                    // Préparation et envoi de l'email pour les invités
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("guest@test.fr"); // Email générique pour les invités
                    mail.To.Add(new MailAddress("app@test.fr"));
                    mail.Subject = contact.Sujet;
                    mail.Body = contact.texte;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("localhost", 587))
                    {
                        smtp.Credentials = new NetworkCredential("guest@test.fr", "123456");
                        smtp.EnableSsl = false; // Devrait être à true en production
                        smtp.Send(mail);
                    }

                    // Redirige vers la page de connexion après l'envoi
                    return RedirectToAction("Index", "Access");
                }
                else
                {
                    // Redirige vers la page de contact en cas d'erreur
                    return RedirectToAction("Contact", "Pieds");
                }
            }
            catch (Exception ex)
            {
                // Gère les erreurs et affiche un message d'erreur
                ViewData["ErrorMessage"] = "Erreur lors de l'envoi du mail : " + ex.Message;
                return RedirectToAction("Contact", "Pieds");
            }

            // Redirige par défaut vers la page d'accueil
            return RedirectToAction("Index", "Acceuil");
        }
    }
}