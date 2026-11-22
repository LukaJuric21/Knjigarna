using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Test.Controllers;
using Test.Data;
using Test.Models;
using Test.Services;

namespace Knjigarna.Tests;

[TestClass]
public sealed class KnjigeControllerTests
{
    private SqliteConnection _povezava = null!;
    private KnjigarnaContext _context = null!;
    private KnjigeController _controller = null!;

    [TestInitialize]
    public void PripraviTest()
    {
        _povezava = new SqliteConnection("Data Source=:memory:");
        _povezava.Open();

        var options = new DbContextOptionsBuilder<KnjigarnaContext>()
            .UseSqlite(_povezava)
            .Options;

        _context = new KnjigarnaContext(options);
        _context.Database.EnsureCreated();

        _controller = new KnjigeController(
            _context, new KnjigaService());
    }

    [TestCleanup]
    public void PocistiTest()
    {
        _context.Dispose();
        _povezava.Dispose();
    }

    private async Task PreveriZavrnitev(
        DodajKnjigoZahteva zahteva,
        string pricakovanoSporocilo)
    {
        var rezultat = await _controller.DodajKnjigo(zahteva);

        Assert.IsInstanceOfType(
            rezultat, typeof(BadRequestObjectResult));

        var odgovor = (BadRequestObjectResult)rezultat;

        Assert.AreEqual(pricakovanoSporocilo, odgovor.Value);
        Assert.AreEqual(0, await _context.Knjige.CountAsync());
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_VeljavniPodatki_ShraniKnjigoInZanre()
    {
        var fantazija = new Zanr { Naziv = "Fantazija" };
        var pustolovscina = new Zanr { Naziv = "Pustolovščina" };

        _context.Zanri.AddRange(fantazija, pustolovscina);
        await _context.SaveChangesAsync();

        var zahteva = new DodajKnjigoZahteva
        {
            Naslov = "  Hobit  ",
            Avtor = "  J. R. R. Tolkien  ",
            Opis = "Bilbova pustolovščina.",
            Cena = 12.99m,
            ZanrIds = new List<int>
            {
                fantazija.Id,
                pustolovscina.Id,
                fantazija.Id
            }
        };

        var rezultat = await _controller.DodajKnjigo(zahteva);

        Assert.IsInstanceOfType(rezultat, typeof(ObjectResult));
        var odgovor = (ObjectResult)rezultat;
        Assert.AreEqual(201, odgovor.StatusCode);

        var podatkiOdgovora =
        JsonSerializer.SerializeToElement(odgovor.Value);

        Assert.AreEqual(
            "Hobit",
            podatkiOdgovora.GetProperty("Naslov").GetString());

        var vrnjeniZanri = podatkiOdgovora
            .GetProperty("ZanrIds")
            .EnumerateArray()
            .Select(z => z.GetInt32())
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { fantazija.Id, pustolovscina.Id },
            vrnjeniZanri);

        _context.ChangeTracker.Clear();

        var knjige = await _context.Knjige
            .Include(k => k.Zanri)
            .ToListAsync();

        Assert.HasCount(1, knjige);

        var knjiga = knjige[0];

        Assert.IsGreaterThan(0, knjiga.Id);
        Assert.AreEqual("Hobit", knjiga.Naslov);
        Assert.AreEqual("J. R. R. Tolkien", knjiga.Avtor);
        Assert.AreEqual("Bilbova pustolovščina.", knjiga.Opis);
        Assert.AreEqual(12.99m, knjiga.Cena);
        Assert.AreNotEqual(default(DateTime), knjiga.DatumDodajanja);

        // Podvojeni ID žanra ne sme ustvariti dodatne povezave.
        CollectionAssert.AreEquivalent(
            new[] { fantazija.Id, pustolovscina.Id },
            knjiga.Zanri.Select(z => z.Id).ToArray());
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_PrazenNaslov_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = " ",
                Avtor = "Avtor"
            },
            "Naslov in avtor sta obvezna.");
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_PrazenAvtor_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = "Hobit",
                Avtor = " "
            },
            "Naslov in avtor sta obvezna.");
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_NegativnaCena_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = "Hobit",
                Avtor = "Tolkien",
                Cena = -1
            },
            "Cena ne sme biti negativna.");
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_PrazenSeznamZanrov_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = "Hobit",
                Avtor = "Tolkien",
                ZanrIds = new List<int>()
            },
            "Izberi vsaj en žanr.");
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_NullSeznamZanrov_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = "Hobit",
                Avtor = "Tolkien",
                ZanrIds = null!
            },
            "Izberi vsaj en žanr.");
    }

    [TestMethod]
    public async Task Test_DodajKnjigo_NeobstojecZanr_ZavrneVnos()
    {
        await PreveriZavrnitev(
            new DodajKnjigoZahteva
            {
                Naslov = "Hobit",
                Avtor = "Tolkien",
                ZanrIds = new List<int> { 999 }
            },
            "Eden ali več izbranih žanrov ne obstaja.");
    }

    [TestMethod]
    public async Task Test_PridobiKnjige_Iskanje_VrneKnjigoZanreInPodatke()
    {
        var zanr = new Zanr { Naziv = "Fantazija" };

        _context.Knjige.AddRange(
            new Knjiga
            {
                Naslov = "Hobit",
                Avtor = "Tolkien",
                Opis = "Bilbova pustolovščina.",
                Cena = 12.99m,
                Zanri = new List<Zanr> { zanr }
            },
            new Knjiga
            {
                Naslov = "Druga knjiga",
                Avtor = "Drug avtor"
            }
        );

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var rezultat = await _controller.PridobiKnjige("HOB");

        Assert.IsInstanceOfType(rezultat, typeof(OkObjectResult));
        var odgovor = (OkObjectResult)rezultat;
        var podatki = JsonSerializer.SerializeToElement(odgovor.Value);

        Assert.AreEqual(1, podatki.GetArrayLength());

        var knjiga = podatki[0];

        Assert.AreEqual("Hobit", knjiga.GetProperty("Naslov").GetString());
        Assert.AreEqual("Tolkien", knjiga.GetProperty("Avtor").GetString());
        Assert.AreEqual(
            "Bilbova pustolovščina.",
            knjiga.GetProperty("Opis").GetString());
        Assert.AreEqual(12.99m, knjiga.GetProperty("Cena").GetDecimal());

        var zanri = knjiga.GetProperty("Zanri");

        Assert.AreEqual(1, zanri.GetArrayLength());
        Assert.AreEqual(zanr.Id, zanri[0].GetProperty("Id").GetInt32());
        Assert.AreEqual(
            "Fantazija", zanri[0].GetProperty("Naziv").GetString());
    }

    [TestMethod]
    public async Task Test_PridobiKnjige_PraznaBaza_VrnePrazenSeznam()
    {
        var rezultat = await _controller.PridobiKnjige(null);

        Assert.IsInstanceOfType(rezultat, typeof(OkObjectResult));
        var odgovor = (OkObjectResult)rezultat;
        var podatki = JsonSerializer.SerializeToElement(odgovor.Value);

        Assert.AreEqual(0, podatki.GetArrayLength());
    }
}