using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Datas
{
    public class SatrancDbContext : DbContext
    {
        public SatrancDbContext(DbContextOptions<SatrancDbContext> options)
           : base(options)
        {
        }
        public DbSet<Oyun> Oyunlar { get; set; }
        public DbSet<Oyuncu> Oyuncular { get; set; }
        public DbSet<Tas> Taslar { get; set; }
        public DbSet<Hamle> Hamleler { get; set; }        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // ConfigurationBuilder kullanıldığında burada ek yapılandırma yapılabilir
            // Ancak Program.cs'de zaten DbContext servis olarak eklenmiştir
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Oyun ↔ Oyuncu (Beyaz ve Siyah Oyuncular)
            modelBuilder.Entity<Oyun>()
                .HasOne(o => o.BeyazOyuncu)
                .WithMany()  // Oyuncu sınıfında ICollection<Hamle> ve ICollection<Tas> var
                .HasForeignKey(o => o.BeyazOyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Oyun>()
                .HasOne(o => o.SiyahOyuncu)
                .WithMany()  // Oyuncu sınıfında ICollection<Hamle> ve ICollection<Tas> var
                .HasForeignKey(o => o.SiyahOyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hamle ↔ Oyun
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyun)
                .WithMany(o => o.Hamleler)  // Oyun sınıfında ICollection<Hamle> var
                .HasForeignKey(h => h.OyunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hamle ↔ Oyuncu
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyuncu)
                .WithMany(o => o.Hamleler)  // Oyuncu sınıfında ICollection<Hamle> var
                .HasForeignKey(h => h.OyuncuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Taş ↔ Oyun
            modelBuilder.Entity<Tas>()
                .HasOne(t => t.Oyun)
                .WithMany(o => o.Taslar)  // Oyun sınıfında ICollection<Tas> var
                .HasForeignKey(t => t.OyunId)
                .OnDelete(DeleteBehavior.Restrict);

            // Taş ↔ Oyuncu
            modelBuilder.Entity<Tas>()
                .HasOne(t => t.Oyuncu)
                .WithMany(o => o.Taslar)  // Oyuncu sınıfında ICollection<Tas> var
                .HasForeignKey(t => t.OyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Hamle>()
       .HasOne(h => h.Tas)  // h.Tas navigation property kullanın
       .WithMany()          // Eğer Tas sınıfında Hamleler koleksiyonu yoksa WithMany()
       .HasForeignKey(h => h.TasId)  // h.TasId foreign key kullanın
       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}