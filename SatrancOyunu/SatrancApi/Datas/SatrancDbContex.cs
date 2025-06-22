using Microsoft.EntityFrameworkCore;
using SatrancApi.Entities.Models;
using SatrancApi.Services;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Datas
{
    public class SatrancDbContext : DbContext
    {
        private readonly ICurrentUserService? _currentUserService;

        public SatrancDbContext(DbContextOptions<SatrancDbContext> options,
                              ICurrentUserService? currentUserService = null)
           : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Oyun> Oyunlar { get; set; }
        public DbSet<Oyuncu> Oyuncular { get; set; }
        public DbSet<Tas> Taslar { get; set; }
        public DbSet<Hamle> Hamleler { get; set; }
        //  AUTOMATİK AUDİT TRACKİNG

        //  SaveChangesAsync metodunu düzelt
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //  ÖNCE KULLANICI ADI, SONRA EMAIL, EN SON SYSTEM
            var currentUser = _currentUserService?.GetCurrentUserName()
                           ?? _currentUserService?.GetCurrentUserEmail()
                           ?? "System";

            var now = DateTime.Now;

            //
            Console.WriteLine($"SaveChanges - Current User: '{currentUser}' at {now}");

            var entries = ChangeTracker.Entries<BaseEntitiy>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedBy = currentUser;
                        Console.WriteLine($" CREATED by: {currentUser}");
                        break;

                    case EntityState.Modified:
                        entry.Property(nameof(BaseEntitiy.CreatedDate)).IsModified = false;
                        entry.Property(nameof(BaseEntitiy.CreatedBy)).IsModified = false;

                        entry.Entity.UpdatedDate = now;
                        entry.Entity.UpdatedBy = currentUser;
                        Console.WriteLine($" UPDATED by: {currentUser}");
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedDate = now;
                        entry.Entity.DeletedBy = currentUser;
                        Console.WriteLine($" DELETED by: {currentUser}");
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // SoftDelete metodunu düzelt
        public void SoftDelete<T>(T entity) where T : BaseEntitiy
        {
            var currentUser = _currentUserService?.GetCurrentUserName()
                           ?? _currentUserService?.GetCurrentUserEmail()
                           ?? "System";

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.Now;
            entity.DeletedBy = currentUser;

            Console.WriteLine($" Soft Delete: {typeof(T).Name} by {currentUser}");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //  SOFT DELETE QUERY FILTER
            modelBuilder.Entity<Oyuncu>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<Oyun>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<Tas>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Hamle>().HasQueryFilter(h => !h.IsDeleted);

            // Oyun - Oyuncu (Beyaz ve Siyah Oyuncular)
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

            // Hamle-Oyun
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyun)
                .WithMany(o => o.Hamleler)
                .HasForeignKey(h => h.OyunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hamle ↔ Oyuncu - 
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Oyuncu)
                .WithMany(o => o.Hamleler)
                .HasForeignKey(h => h.OyuncuId)
                .OnDelete(DeleteBehavior.Restrict);  

            // Hamle - Tas 
            modelBuilder.Entity<Hamle>()
                .HasOne(h => h.Tas)
                .WithMany(t => t.Hamleler)  
                .HasForeignKey(h => h.TasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Taş -Oyun
            modelBuilder.Entity<Tas>()
                .HasOne(t => t.Oyun)
                .WithMany(o => o.Taslar)
                .HasForeignKey(t => t.OyunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Taş- Oyuncu
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