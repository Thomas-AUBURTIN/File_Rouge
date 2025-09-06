using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class ConnectionViewModel
    {
        public Utilisateur Utilisateur { get; set; } = new Utilisateur();

        public List<Jeux> ListJeux { get; set; } = new List<Jeux>();

        String message = "";

    }
}
