using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Jeux
    {
        public int  jeuid { get; set; }
        [Required(ErrorMessage = "Le titre est obligatoire")]
        public string? titre { get; set; }
        [Required(ErrorMessage = "La description est obligatoire")]
        public string? description { get; set; }
        [Required(ErrorMessage = "L'image est obligatoire")]
        public string? image { get; set; }
        [Required(ErrorMessage = "Indiquer le nombre de joueur")]
        public int? nombrejoueursrecommandes { get; set; }
        [Required(ErrorMessage = "Indiquer le temps de jeu")]
        public double? tempsdejeumoyen { get; set; }
        public DateTime? dateajout { get; set; }

        public List<Theme> themes { get; set; }  = new List<Theme>();
        public List<TypeJeux> types { get; set; } = new List<TypeJeux>();
        public List<Commentaire> commentaires { get; set; } = new List<Commentaire>();
      



    }
}
