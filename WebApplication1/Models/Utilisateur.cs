using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Modèle représentant un utilisateur de l'application
    public class Utilisateur
    {
        // Identifiant unique de l'utilisateur
        public int utilisateurId { get; set; }

        // Nom de l'utilisateur (champ obligatoire, longueur 1 à 100)
        [Required(ErrorMessage = "Indiquer votre nom ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? nom { get; set; }

        // Pseudo de l'utilisateur (champ obligatoire, longueur 1 à 100)
        [Required(ErrorMessage = "Indiquer votre pseudo ")]
        [MinLength(1)]
        [MaxLength(100)]
        public string? pseudo { get; set; }

        // Email de l'utilisateur (champ obligatoire, format email)
        [Required(ErrorMessage = "Indiquer votre email ")]
        [DataType(DataType.EmailAddress)]
        public string? email { get; set; }

        // Mot de passe de l'utilisateur (champ obligatoire, min 6 caractères)
        [Required(ErrorMessage = "Indiquer votre mot de passe ")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        public string? motdePasse { get; set; }

        // Numéro de téléphone de l'utilisateur (champ obligatoire, format téléphone)
        [Required(ErrorMessage = "Indiquer votre numéro de telephone ")]
        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
        [DataType(DataType.PhoneNumber)]
        public string? Telephone { get; set; }

        // Date d'inscription de l'utilisateur (par défaut à la création)
        public DateTime? dateInscription { get; set; } = DateTime.Now;

        // Indique si l'utilisateur est administrateur (par défaut false)
        public bool? administrateur { get; set; } = false;
    }
}