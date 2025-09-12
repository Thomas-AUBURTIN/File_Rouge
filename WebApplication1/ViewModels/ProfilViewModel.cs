using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class ProfilViewModel
    {
        public Utilisateur utilisateur { get; set; }

        public List<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    }
}
