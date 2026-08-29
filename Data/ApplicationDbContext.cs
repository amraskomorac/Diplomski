using Microsoft.EntityFrameworkCore;
using Diplomski.Models;

namespace Diplomski.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<TokenZaResetLozinke> TokeniZaResetLozinke { get; set; }
        public DbSet<Vozilo> Vozila { get; set; }
        public DbSet<Servis> Servisi { get; set; }
        public DbSet<Gorivo> Goriva { get; set; }
        public DbSet<HistorijaRegistracije> HistorijaRegistracija { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vozilo>()
                .HasOne(v => v.Korisnik)
                .WithMany(k => k.Vozila)
                .HasForeignKey(v => v.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Servis>()
                .HasOne(s => s.Vozilo)
                .WithMany()
                .HasForeignKey(s => s.VoziloId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Servis>()
                .HasOne(s => s.Korisnik)
                .WithMany(k => k.Servisi)
                .HasForeignKey(s => s.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistorijaRegistracije>()
                .HasOne(h => h.Vozilo)
                .WithMany()
                .HasForeignKey(h => h.VoziloId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
