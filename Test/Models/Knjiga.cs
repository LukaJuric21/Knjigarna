namespace Test.Models
{
    public class Knjiga
    {
        public int Id { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Avtor { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public decimal Cena { get; set; }
        public DateTime DatumDodajanja { get; set; } = DateTime.UtcNow;

        public List<Zanr> Zanri { get; set; } = new();
    }
}
