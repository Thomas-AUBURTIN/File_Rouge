namespace WebApplication1.Models
{
    public class Theme
    {
        public int themeId { get; }
        public string? nom { get; set; }
        public string? description { get; set; }
        public List<Jeux> jeux { get; set; } = new List<Jeux>();
    }
}
