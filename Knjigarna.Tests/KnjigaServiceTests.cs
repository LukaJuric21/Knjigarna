using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Models;
using Test.Services;

namespace Knjigarna.Tests;

[TestClass]
public sealed class KnjigaServiceTests
{
    private static List<Knjiga> PripraviKnjige()
    {
        return new List<Knjiga>
        {
            new Knjiga
            {
                Id = 1,
                Naslov = "Harry Potter in kamen modrosti"
            },
            new Knjiga
            {
                Id = 2,
                Naslov = "Harry Potter in dvorana skrivnosti"
            },
            new Knjiga
            {
                Id = 3,
                Naslov = "Hobit"
            }
        };
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_DelNaslova_VrneUstrezniKnjigi()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "Potter");

        CollectionAssert.AreEquivalent(
            new[] { 1, 2 },
            rezultat.Select(k => k.Id).ToArray());
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_CelotenNaslov_VrneEnoKnjigo()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "Hobit");

        Assert.HasCount(1, rezultat);
        Assert.AreEqual(3, rezultat[0].Id);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_VelikeCrke_VrneUstrezniKnjigi()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "HARRY");

        CollectionAssert.AreEquivalent(
            new[] { 1, 2 },
            rezultat.Select(k => k.Id).ToArray());
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_PresledkiObVnosu_JihOdstrani()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "  Hobit  ");

        Assert.HasCount(1, rezultat);
        Assert.AreEqual(3, rezultat[0].Id);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_NeobstojecNaslov_VrnePrazenSeznam()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "Neobstojeca");

        Assert.IsEmpty(rezultat);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_PrazenNiz_VrneVseKnjige()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "");

        CollectionAssert.AreEquivalent(knjige, rezultat);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_Null_VrneVseKnjige()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, null);

        CollectionAssert.AreEquivalent(knjige, rezultat);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_SamoPresledki_VrneVseKnjige()
    {
        var service = new KnjigaService();
        var knjige = PripraviKnjige();

        var rezultat = service.IsciPoNaslovu(knjige, "   ");

        CollectionAssert.AreEquivalent(knjige, rezultat);
    }

    [TestMethod]
    public void Test_IsciPoNaslovu_PrazenSeznam_VrnePrazenSeznam()
    {
        var service = new KnjigaService();
        var knjige = new List<Knjiga>();

        var rezultat = service.IsciPoNaslovu(knjige, "Harry");

        Assert.IsEmpty(rezultat);
    }
}