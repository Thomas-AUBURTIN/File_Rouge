using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class EditeurJeuViewModel
    {
        public Jeux Jeu { get; set; }

        public List<SelectListItem> ListTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListThemes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListJoueur { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListTemps { get; set; } = new List<SelectListItem>();
        //public List<int> SelectedThemes { get; set; }
        //public List<int> SelectedTypes { get; set; }
        //public int SelectedJoueur { get; set; }
        //public int Selectedtemps { get; set; }

        public required string action { get; init; }

        public required string titre { get; init; }

        public int? idJeu { get; set; } = null;
    }
}
