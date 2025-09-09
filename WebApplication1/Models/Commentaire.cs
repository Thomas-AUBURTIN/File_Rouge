using System.ComponentModel.DataAnnotations; // Pour les annotations de validation

namespace WebApplication1.Models
{
    // Modèle représentant un commentaire laissé par un utilisateur sur un jeu
    public class Commentaire
    {
        // Identifiant du jeu concerné par le commentaire
        public int jeuId { get; set; }

        public String utilisateurNom { get; set; }
        // Identifiant de l'utilisateur ayant laissé le commentaire
        public int utilisateurId { get; set; }

        // Texte du commentaire (champ obligatoire)
        [Required(ErrorMessage = "Entrer votre commentaires")]
        public string? commentaire { get; set; }

        // Date à laquelle le commentaire a été laissé
        public DateTime? datecommentaires { get; set; } = DateTime.UtcNow;

        [Range(0, 5)]
        [Required(ErrorMessage = "Entrer votre note")]
        public int note { get; set; }
    }
}