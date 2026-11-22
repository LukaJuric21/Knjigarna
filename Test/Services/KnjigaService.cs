using Test.Models;

namespace Test.Services;

public class KnjigaService
{
    public List<Knjiga> IsciPoNaslovu(
        IEnumerable<Knjiga> knjige,
        string? naslov)
    {
        if (string.IsNullOrWhiteSpace(naslov))
        {
            return knjige.ToList();
        }

        string iskaniNaslov = naslov.Trim();

        return knjige
            .Where(k => k.Naslov.Contains(
                iskaniNaslov,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}