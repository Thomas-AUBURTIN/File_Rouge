using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Jeux
    {
        //ceci est un test
        public int jeuid { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        public string? titre { get; set; }

        [Required(ErrorMessage = "La description est obligatoire")]
        public string? description { get; set; }

        [Required(ErrorMessage = "L'image est obligatoire")]
        public string? image { get; set; }
        public IFormFile? imageFile { get; set; } // pour le formulaire

        [Required(ErrorMessage = "Indiquer le nombre de joueur")]
        public int? nombrejoueursrecommandes { get; set; }

        [Required(ErrorMessage = "Indiquer le temps de jeu")]
        public double? tempsjeumoyen { get; set; }

        public DateTime? dateajout { get; set; }

        public List<Theme> themes { get; set; } = new List<Theme>();
        public List<JeuxType> types { get; set; } = new List<JeuxType>();
        public List<Commentaire> commentaires { get; set; } = new List<Commentaire>();
        public List<int> idTheme { get; set; } = new List<int>();
        public List<int> idType { get; set; } = new List<int>();
    }
}