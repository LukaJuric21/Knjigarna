using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Data;
using Test.Models;

namespace Knjigarna.Tests;

[TestClass]
public sealed class ApiTests
{
    [TestMethod]
    [DataRow("Development")]
    [DataRow("Production")]
    public async Task Test_Api_ZagonInBranjeZanrov_VrnePodatke(
        string okolje)
    {
        // Ločena baza, ki obstaja samo med tem testom.
        using var povezava =
            new SqliteConnection("Data Source=:memory:");

        await povezava.OpenAsync();

        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(okolje);

                builder.ConfigureServices(services =>
                {
                    // Zamenjamo povezavo do običajne baze
                    // s povezavo do začasne testne baze.
                    services.RemoveAll<
                        DbContextOptions<KnjigarnaContext>>();

                    services.RemoveAll<
                        IDbContextOptionsConfiguration<KnjigarnaContext>>();

                    services.AddDbContext<KnjigarnaContext>(options =>
                        options.UseSqlite(povezava));
                });
            });

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

        // Pripravimo tabelo in začetni podatek.
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<KnjigarnaContext>();

            await context.Database.EnsureCreatedAsync();

            context.Zanri.Add(new Zanr
            {
                Naziv = "Fantazija"
            });

            await context.SaveChangesAsync();
        }

        // Zahteva potuje skozi aplikacijo do kontrolerja.
        using var odgovor = await client.GetAsync("/api/zanri");

        Assert.AreEqual(HttpStatusCode.OK, odgovor.StatusCode);

        var vsebina = await odgovor.Content.ReadAsStringAsync();

        using var dokument = JsonDocument.Parse(vsebina);
        var zanri = dokument.RootElement;

        Assert.AreEqual(1, zanri.GetArrayLength());
        Assert.AreEqual(
            "Fantazija",
            zanri[0].GetProperty("naziv").GetString());
    }
}