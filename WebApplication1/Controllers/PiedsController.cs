using Microsoft.AspNetCore.Mvc; // Pour les contrôleurs MVC

namespace WebApplication1.Controllers
{
    // Contrôleur pour la gestion des pages de pied de page (footer)
    public class PiedsController : Controller
    {
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
        public IActionResult Contact()
        {
            return View(); // Retourne la vue associée à Contact
        }
    }
}