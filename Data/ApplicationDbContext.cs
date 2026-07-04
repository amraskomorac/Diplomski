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
    }
}