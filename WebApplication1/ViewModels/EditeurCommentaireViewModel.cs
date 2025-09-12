using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
namespace WebApplication1.ViewModels
{

    public class EditeurCommentaireViewModel
    {
        public Commentaire Commentaire { get; set; }
        public string Titre { get; set; }
        public string Action { get; set; }
    }
}
