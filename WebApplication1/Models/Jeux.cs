namespace WebApplication1.Models
{
    public class Jeux
    {
        public int  jeuid { get; }
        public string? titre { get; set; }
        public string? description { get; set; }
        public string? image { get; set; }
        public int? nombrejoueursrecommandes { get; set; }
        public double? tempsdejeumoyen { get; set; }
        public DateTime? dateajout { get; set; }


    }
}
