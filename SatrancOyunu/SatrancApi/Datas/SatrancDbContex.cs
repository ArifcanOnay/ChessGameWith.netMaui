using Microsoft.EntityFrameworkCore;
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
        public DbSet<Hamle> Hamleler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Oyun ↔ Oyuncu (Beyaz ve Siyah Oyuncular)
            modelBuilder.Entity<Oyun>()
                .HasOne(o => o.BeyazOyuncu)
                .WithMany()
                .HasForeignKey(o => o.BeyazOyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Oyun>()
                .HasOne(o => o.SiyahOyuncu)
                .WithMany()
                .HasForeignKey(o => o.SiyahOyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hamle ↔ Oyun
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyun)
                .WithMany(o => o.Hamleler)
                .HasForeignKey(h => h.OyunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hamle ↔ Oyuncu - ✅ DÜZELTİLDİ
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyuncu)
                .WithMany(o => o.Hamleler)
                .HasForeignKey(h => h.OyuncuId)
                .OnDelete(DeleteBehavior.Restrict);  // ✅ Restrict yapıldı

            // Hamle ↔ Tas - ✅ DÜZELTİLDİ
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Tas)
                .WithMany(t => t.Hamleler)  // ✅ WithMany(t => t.Hamleler) eklendi
                .HasForeignKey(h => h.TasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Taş ↔ Oyun
            modelBuilder.Entity<Tas>()
                .HasOne(t => t.Oyun)
                .WithMany(o => o.Taslar)
                .HasForeignKey(t => t.OyunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Taş ↔ Oyuncu
            modelBuilder.Entity<Tas>()
                .HasOne(t => t.Oyuncu)
                .WithMany(o => o.Taslar)
                .HasForeignKey(t => t.OyuncuId)
                .OnDelete(DeleteBehavior.Restrict);

            // İndeksler
            modelBuilder.Entity<Hamle>()
                .HasIndex(h => new { h.OyunId, h.HamleTarihi })
                .HasDatabaseName("IX_Hamle_OyunId_Tarih");

            modelBuilder.Entity<Tas>()
                .HasIndex(t => new { t.OyunId, t.AktifMi })
                .HasDatabaseName("IX_Tas_OyunId_Aktif");
        }
    }
}