namespace WebApplication1.Models
{
    public class Commentaire
    {
        public int jeuId { get; }
        public int utilisateurId { get; }
        public string? commentaire { get; set; }
        public DateTime? datecommentaires { get; set; }

    }
}
