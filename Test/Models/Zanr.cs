using System.Security.AccessControl;

namespace Test.Models
{
    public class Zanr
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;

        public List<Knjiga> Knjige { get; set; } = new();
    }
}
