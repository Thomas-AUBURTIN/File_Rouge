using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Utilisateur
    {
        public int utilisateurId {  get; set; }
        [Required(ErrorMessage = "Indiquer votre nom ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? nom { get; set; }
        [Required(ErrorMessage = "Indiquer votre pseudo ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? pseudo { get; set; }
        [Required(ErrorMessage = "Indiquer votre email ")]
        public string? email { get; set; }
        [Required(ErrorMessage = "Indiquer votre mot de passe ")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string? motdePasse { get; set; }
        public string? Telephone { get; set; }
        public DateTime? dateInscription { get; set; }
        public bool? administrateur { get; set; }
        
    }
}
