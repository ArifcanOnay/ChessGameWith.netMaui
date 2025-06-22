using SatrancApi.Entities.Models;

namespace SatrancAPI.Entities.Models
{
    public class Oyun:BaseEntitiy
    {
        public Guid OyunId { get; set; }
        public Guid BeyazOyuncuId { get; set; }
        public Guid SiyahOyuncuId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public int BeyazSkor { get; set; }
        public int SiyahSkor { get; set; }
        public TimeSpan? BeyazKalanSure { get; set; }
        public TimeSpan? SiyahKalanSure { get; set; }
        public Durum? Durum { get; set; }  // Örn: "Oynanıyor", "Bitti",
        public Oyuncu ?BeyazOyuncu { get; set; }
        public Oyuncu? SiyahOyuncu { get; set; }
        public int SiraKimin { get; set; } = 0; // 0: Beyaz, 1: Siyah
        public int ToplamHamleSayisi { get; set; } = 0;
        public bool SahDurumu { get; set; } = false;
        public string? KazananOyuncu { get; set; }
        public string? BitisNedeni { get; set; } // "Şah Mat", "Berabere", "Terk"

        public ICollection<Hamle>? Hamleler { get; set; }  // Oyun ile birden fazla hamle ilişkisi
        public ICollection<Tas> ?Taslar { get; set; }  // Oyun ile birden fazla taş ilişkisi
        
    }
}
