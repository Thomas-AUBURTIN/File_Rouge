using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    // ViewModel utilisé pour l'édition ou la création d'un jeu
    public class EditeurJeuViewModel
    {
        // Jeu à éditer ou à créer
        public Jeux Jeu { get; set; }

        // Liste des types de jeux disponibles (pour les menus déroulants)
        public List<SelectListItem> ListTypes { get; set; } = new List<SelectListItem>();

        // Liste des thèmes disponibles (pour les menus déroulants)
        public List<SelectListItem> ListThemes { get; set; } = new List<SelectListItem>();

        // Liste des nombres de joueurs disponibles (pour les menus déroulants)
        public List<SelectListItem> ListJoueur { get; set; } = new List<SelectListItem>();

        // Liste des temps de jeu disponibles (pour les menus déroulants)
        public List<SelectListItem> ListTemps { get; set; } = new List<SelectListItem>();

        // Action à réaliser ("Editer" ou "Nouveau")
        public required string action { get; init; }

        // Titre de la page ou du formulaire
        public required string titre { get; init; }

        // Identifiant du jeu (null si création)
        public int? idJeu { get; set; } = null;
    }
}
