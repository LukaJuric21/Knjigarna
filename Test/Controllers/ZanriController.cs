using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;

namespace Test.Controllers;

[ApiController]
[Route("api/zanri")]
public class ZanriController : ControllerBase
{
    private readonly KnjigarnaContext _context;

    public ZanriController(KnjigarnaContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> PridobiZanre()
    {
        var zanri = await _context.Zanri
            .Select(z => new
            {
                z.Id,
                z.Naziv
            })
            .ToListAsync();

        return Ok(zanri);
    }

    [HttpPost]
    public async Task<IActionResult> DodajZanr(
        [FromBody] DodajZanrZahteva zahteva)
    {
        if (string.IsNullOrWhiteSpace(zahteva.Naziv))
        {
            return BadRequest("Naziv žanra je obvezen.");
        }

        var zanr = new Zanr
        {
            Naziv = zahteva.Naziv.Trim()
        };

        _context.Zanri.Add(zanr);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            zanr.Id,
            zanr.Naziv
        });
    }
}

public class DodajZanrZahteva
{
    public string Naziv { get; set; } = string.Empty;
}