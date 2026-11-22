using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Data;

public class KnjigarnaContext : DbContext
{
    public KnjigarnaContext(
        DbContextOptions<KnjigarnaContext> options) : base(options)
    {
    }

    public DbSet<Knjiga> Knjige { get; set; }
    public DbSet<Zanr> Zanri { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Knjiga>()
            .HasMany(k => k.Zanri)
            .WithMany(z => z.Knjige)
            .UsingEntity(j => j.ToTable("KnjigaZanr"));
    }
}