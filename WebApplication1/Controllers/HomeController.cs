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
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    // Contrôleur pour gérer les erreurs globales et les pages spécifiques comme "Access Denied" ou "Not Found"
    public class HomeController : Controller
    {
        /// <summary>
        /// Gère les erreurs HTTP spécifiques (403, 404, etc.)
        /// </summary>
        /// <param name="statusCode">Code d'état HTTP de l'erreur</param>
        /// <returns>Vue correspondant au code d'erreur</returns>
        [Route("/Home/HandleError/{statusCode}")]
        public IActionResult HandleError([FromRoute] int statusCode)
        {
            // Vérifie le code d'erreur et retourne la vue appropriée
            if (statusCode == 403)
            {
                // Retourne la vue "AccessDenied" pour les erreurs 403 (Accès refusé)
                return View("AccessDenied");
            }
            else if (statusCode == 404)
            {
                // Retourne la vue "NotFound" pour les erreurs 404 (Page non trouvée)
                return View("NotFound");
            }
            else
            {
                // Retourne une vue générique "Error" pour les autres erreurs
                return View("Error");
            }
        }

        /// <summary>
        /// Affiche une vue générique pour les erreurs
        /// </summary>
        /// <param name="statusCode">Code d'état HTTP de l'erreur (facultatif)</param>
        /// <returns>Vue Error</returns>
        public IActionResult Error([FromRoute] int statusCode)
        {
            // Retourne une vue générique pour les erreurs
            return View();
        }
    }
}