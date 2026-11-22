using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;
using Test.Services;

namespace Test.Controllers;

[ApiController]
[Route("api/knjige")]
public class KnjigeController : ControllerBase
{
    private readonly KnjigarnaContext _context;
    private readonly KnjigaService _service;

    public KnjigeController(
        KnjigarnaContext context,
        KnjigaService service)
    {
        _context = context;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> PridobiKnjige(
        [FromQuery] string? naslov)
    {
        var knjige = await _context.Knjige
            .AsNoTracking()
            .Include(k => k.Zanri)
            .ToListAsync();

        var rezultat = _service.IsciPoNaslovu(knjige, naslov);

        return Ok(rezultat.Select(k => new
        {
            k.Id,
            k.Naslov,
            k.Avtor,
            k.Opis,
            k.Cena,
            k.DatumDodajanja,
            Zanri = k.Zanri.Select(z => new
            {
                z.Id,
                z.Naziv
            })
        }));
    }

    [HttpPost]
    public async Task<IActionResult> DodajKnjigo(
        [FromBody] DodajKnjigoZahteva zahteva)
    {
        if (string.IsNullOrWhiteSpace(zahteva.Naslov) ||
            string.IsNullOrWhiteSpace(zahteva.Avtor))
        {
            return BadRequest("Naslov in avtor sta obvezna.");
        }

        if (zahteva.Cena < 0)
        {
            return BadRequest("Cena ne sme biti negativna.");
        }

        if (zahteva.ZanrIds == null || zahteva.ZanrIds.Count == 0)
        {
            return BadRequest("Izberi vsaj en žanr.");
        }

        var zanrIds = zahteva.ZanrIds.Distinct().ToList();

        var zanri = await _context.Zanri
            .Where(z => zanrIds.Contains(z.Id))
            .ToListAsync();

        if (zanri.Count != zanrIds.Count)
        {
            return BadRequest("Eden ali več izbranih žanrov ne obstaja.");
        }

        var knjiga = new Knjiga
        {
            Naslov = zahteva.Naslov.Trim(),
            Avtor = zahteva.Avtor.Trim(),
            Opis = zahteva.Opis,
            Cena = zahteva.Cena,
            Zanri = zanri
        };

        _context.Knjige.Add(knjiga);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            knjiga.Id,
            knjiga.Naslov,
            knjiga.Avtor,
            knjiga.Cena,
            ZanrIds = zanri.Select(z => z.Id)
        });
    }
}

public class DodajKnjigoZahteva
{
    public string Naslov { get; set; } = string.Empty;
    public string Avtor { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cena { get; set; }
    public List<int> ZanrIds { get; set; } = new();
}