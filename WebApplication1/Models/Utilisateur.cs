namespace WebApplication1.Models
{
    public class Utilisateur
    {
        public int utilisateurId {  get; }
        public string? nom { get; set; }
        public string? email { get; set; }
        public string? motdePasse { get; set; }
        public string? Telephone { get; set; }
        public DateTime? dateInscription { get; set; }
        public bool? administrateur { get; set; }
    }
}
