using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    // Contrôleur pour gérer les fonctionnalités de la page d'accueil
    [Authorize(Roles = "User")] // Restreint l'accès aux utilisateurs ayant le rôle "User"
    public class AcceuilController : Controller
    {
        /// <summary>
        /// Action affichant la page d'accueil
        /// </summary>
        /// <returns>Vue Index</returns>
        public IActionResult Index()
        {
            // Retourne la vue de la page d'accueil
            return View();
        }

        /// <summary>
        /// Action affichant la page d'erreur
        /// </summary>
        /// <returns>Vue Error avec les informations sur l'erreur</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Désactive la mise en cache pour cette action
        public IActionResult Error()
        {
            // Retourne la vue Error avec un modèle contenant l'ID de la requête
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}