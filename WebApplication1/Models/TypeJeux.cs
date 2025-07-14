namespace WebApplication1.Models
{
    public class TypeJeux
    {
        public int typeId { get; }
        public string? nom { get; set; }
        public string? description { get; set; }
        public List<Jeux> jeux { get; set; } = new List <Jeux> ();

    }
}
