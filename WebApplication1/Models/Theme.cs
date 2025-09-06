using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Modèle représentant un thème de jeu (ex : aventure, science-fiction, etc.)
    public class Theme
    {
        // Identifiant unique du thème
        public int themeId { get; set; }

        // Nom du thème (champ obligatoire)
        [Required(ErrorMessage = "Indiquer le nom")]
        public string? nom { get; set; }

        // Description du thème (champ obligatoire)
        [Required(ErrorMessage = "Indiquer la description")]
        public string? description { get; set; }

        // Liste des jeux associés à ce thème
        public List<Jeux> jeux { get; set; } = new List<Jeux>();
    }
}