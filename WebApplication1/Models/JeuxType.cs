using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Modèle représentant un type de jeu (ex : stratégie, ambiance, etc.)
    public class JeuxType
    {
        // Identifiant unique du type de jeu
        public int typeId { get; set; }

        // Nom du type de jeu (champ obligatoire)
        [Required(ErrorMessage = "Indiquer le nom")]
        public string? nom { get; set; }

        // Description du type de jeu (champ obligatoire)
        [Required(ErrorMessage = "Indiquer la description")]
        public string? description { get; set; }

        // Liste des jeux associés à ce type
        public List<Jeux> jeux { get; set; } = new List<Jeux>();
    }
}