using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class CatalogueViewModel
    {
        public List<Jeux> ListJeux { get; set; } = new List<Jeux>() ;
        public List<SelectListItem> ListTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListThemes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListJoueur { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListTemps { get; set; } = new List<SelectListItem>();
        public String SelectedTheme { get; set; }
        public String SelectedType { get; set; }
        public String SelectedJoueur { get; set; }
        public String Selectedtemps { get; set; }

    }
}
