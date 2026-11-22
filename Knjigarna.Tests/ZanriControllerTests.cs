using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Test.Controllers;
using Test.Data;
using Test.Models;

namespace Knjigarna.Tests;

[TestClass]
public sealed class ZanriControllerTests
{
    private SqliteConnection _povezava = null!;
    private KnjigarnaContext _context = null!;
    private ZanriController _controller = null!;

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

        _controller = new ZanriController(_context);
    }

    [TestCleanup]
    public void PocistiTest()
    {
        _context.Dispose();
        _povezava.Dispose();
    }

    [TestMethod]
    public async Task Test_DodajZanr_VeljavenNaziv_ShraniZanr()
    {
        var zahteva = new DodajZanrZahteva
        {
            Naziv = "  Fantazija  "
        };

        var rezultat = await _controller.DodajZanr(zahteva);

        Assert.IsInstanceOfType(rezultat, typeof(ObjectResult));
        var odgovor = (ObjectResult)rezultat;
        Assert.AreEqual(201, odgovor.StatusCode);

        // Odstranimo sledene objekte in podatke ponovno preberemo iz baze.
        _context.ChangeTracker.Clear();

        var shranjeni = await _context.Zanri.ToListAsync();

        Assert.HasCount(1, shranjeni);
        Assert.AreEqual("Fantazija", shranjeni[0].Naziv);
        Assert.IsGreaterThan(0, shranjeni[0].Id);
    }

    [TestMethod]
    public async Task Test_DodajZanr_PrazenNaziv_ZavrneVnos()
    {
        var zahteva = new DodajZanrZahteva
        {
            Naziv = "   "
        };

        var rezultat = await _controller.DodajZanr(zahteva);

        Assert.IsInstanceOfType(rezultat, typeof(BadRequestObjectResult));
        var odgovor = (BadRequestObjectResult)rezultat;

        Assert.AreEqual("Naziv žanra je obvezen.", odgovor.Value);
        Assert.AreEqual(0, await _context.Zanri.CountAsync());
    }

    [TestMethod]
    public async Task Test_PridobiZanre_VrneShranjeneZanre()
    {
        _context.Zanri.AddRange(
            new Zanr { Naziv = "Fantazija" },
            new Zanr { Naziv = "Romantika" }
        );

        await _context.SaveChangesAsync();

        var rezultat = await _controller.PridobiZanre();

        Assert.IsInstanceOfType(rezultat, typeof(OkObjectResult));
        var odgovor = (OkObjectResult)rezultat;

        // Kontroler vrne anonimne objekte, zato njihove podatke
        // preverimo prek JSON predstavitve.
        var podatki = JsonSerializer.SerializeToElement(odgovor.Value);

        Assert.AreEqual(2, podatki.GetArrayLength());

        var nazivi = podatki.EnumerateArray()
            .Select(z => z.GetProperty("Naziv").GetString())
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "Fantazija", "Romantika" },
            nazivi);
    }

    [TestMethod]
    public async Task Test_PridobiZanre_PraznaBaza_VrnePrazenSeznam()
    {
        var rezultat = await _controller.PridobiZanre();

        Assert.IsInstanceOfType(rezultat, typeof(OkObjectResult));
        var odgovor = (OkObjectResult)rezultat;

        var podatki = JsonSerializer.SerializeToElement(odgovor.Value);

        Assert.AreEqual(0, podatki.GetArrayLength());
    }
}