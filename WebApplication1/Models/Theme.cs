using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Theme
    {
        public int themeId { get; set; }
        [Required(ErrorMessage = "Indiquer le nom")]
        public string? nom { get; set; }
        [Required(ErrorMessage = "Indiquer la description")]
        public string? description { get; set; }
        public List<Jeux> jeux { get; set; } = new List<Jeux>();
    }
}
