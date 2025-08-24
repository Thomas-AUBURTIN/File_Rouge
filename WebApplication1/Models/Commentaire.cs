using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Commentaire
    {
        public int jeuId { get; set; }
        public int utilisateurId { get; set; }

        [Required(ErrorMessage = "Entrer votre commentaires")]
        public string? commentaire { get; set; }

        public DateTime? datecommentaires { get; set; }
    }
}