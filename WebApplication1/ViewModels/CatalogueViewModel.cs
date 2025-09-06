using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    // ViewModel utilisé pour afficher et filtrer le catalogue de jeux
    public class CatalogueViewModel
    {
        // Liste des jeux à afficher dans le catalogue
        public List<Jeux> ListJeux { get; set; } = new List<Jeux>();

        // Liste des types de jeux disponibles pour le filtre (menu déroulant)
        public List<SelectListItem> ListTypes { get; set; } = new List<SelectListItem>();

        // Liste des thèmes disponibles pour le filtre (menu déroulant)
        public List<SelectListItem> ListThemes { get; set; } = new List<SelectListItem>();

        // Liste des nombres de joueurs disponibles pour le filtre (menu déroulant)
        public List<SelectListItem> ListJoueur { get; set; } = new List<SelectListItem>();

        // Liste des temps de jeu disponibles pour le filtre (menu déroulant)
        public List<SelectListItem> ListTemps { get; set; } = new List<SelectListItem>();

        // Identifiant du thème sélectionné par l'utilisateur
        [Display(Name = "Themes")]
        public int role_id { get; set; }
        public String SelectedTheme { get; set; }

        // Identifiant du type sélectionné par l'utilisateur
        [Display(Name = "Types")]
        public String SelectedType { get; set; }

        // Nombre de joueurs sélectionné par l'utilisateur
        [Display(Name = "Nombres de joueurs")]
        public String SelectedJoueur { get; set; }

        // Temps de jeu sélectionné par l'utilisateur
        [Display(Name = "Temps moyen")]
        public String Selectedtemps { get; set; }
    }
}
