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
        public DbSet<Trosak> Troskovi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vozilo>()
                .HasOne(v => v.Korisnik)
                .WithMany(k => k.Vozila)
                .HasForeignKey(v => v.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trosak>()
                .HasOne(t => t.Korisnik)
                .WithMany(k => k.Troskovi)
                .HasForeignKey(t => t.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
