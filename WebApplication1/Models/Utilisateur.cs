using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Utilisateur
    {
        public int utilisateurId { get; set; }

        [Required(ErrorMessage = "Indiquer votre nom ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? nom { get; set; }

        [Required(ErrorMessage = "Indiquer votre pseudo ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? pseudo { get; set; }

        [Required(ErrorMessage = "Indiquer votre email ")]
        [DataType(DataType.EmailAddress)]
        public string? email { get; set; }

        [Required(ErrorMessage = "Indiquer votre mot de passe ")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        public string? motdePasse { get; set; }

        [Required(ErrorMessage = "Indiquer votre numéro de telephone ")]
        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
        [DataType(DataType.PhoneNumber)]
        public string? Telephone { get; set; }

        public DateTime? dateInscription { get; set; } = DateTime.Now;
        public bool? administrateur { get; set; } = false;
    }
}