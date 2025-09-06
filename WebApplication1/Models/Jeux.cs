using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Modèle représentant un jeu de société dans l'application
    public class Jeux
    {
        // Identifiant unique du jeu
        public int jeuid { get; set; }

        // Titre du jeu (champ obligatoire)

        [Required(ErrorMessage = "Le titre est obligatoire")]
        public string? titre { get; set; }

        // Description du jeu (champ obligatoire)
        [Required(ErrorMessage = "La description est obligatoire")]
        public string? description { get; set; }

        // Nom du fichier image associé au jeu
        public string? image { get; set; }

        // Fichier image uploadé via le formulaire
        public IFormFile? imageFile { get; set; } // pour le formulaire

        // Nombre de joueurs recommandés (champ obligatoire)
        [Required(ErrorMessage = "Indiquer le nombre de joueur")]
        public int? nombrejoueursrecommandes { get; set; }

        // Temps de jeu moyen en minutes (champ obligatoire)
        [Display(Name = "temps de jeu moyen")]
        [Required(ErrorMessage = "Le rôle est requis")]
        public int role_id { get; set; }
        [Required(ErrorMessage = "Indiquer le temps de jeu")]
        public double? tempsjeumoyen { get; set; }

        // Date d'ajout du jeu dans la base
        public DateTime? dateajout { get; set; }

        // Liste des thèmes associés au jeu
        public List<Theme> themes { get; set; } = new List<Theme>();

        // Liste des types associés au jeu
        public List<JeuxType> types { get; set; } = new List<JeuxType>();

        // Liste des commentaires laissés sur le jeu
        public List<Commentaire> commentaires { get; set; } = new List<Commentaire>();

        // Liste des identifiants des thèmes sélectionnés (pour les formulaires)
        public List<int> idThemes { get; set; } = new List<int>();

        // Liste des identifiants des types sélectionnés (pour les formulaires)
        public List<int> idTypes { get; set; } = new List<int>();
    }
}