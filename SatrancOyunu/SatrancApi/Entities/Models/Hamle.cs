using SatrancApi.Entities.Models;

namespace SatrancAPI.Entities.Models
{
    public class Hamle:BaseEntitiy
    {
        public Guid HamleId { get; set; }
        public Guid OyunId { get; set; }
        public Guid OyuncuId { get; set; }
        public Guid TasId { get; set; }
        public TasTuru turu { get; set; }  
        public int BaslangicX { get; set; }
        public int BaslangicY { get; set; }
        public int HedefX { get; set; }
        public int HedefY { get; set; }
        public bool RokMu { get; set; } = false;
        public bool EnPassantMi { get; set; } = false;
        public TasTuru? TerfiEdildigiTas { get; set; } // Piyon terfi
        public bool SahMi { get; set; } = false;
        public bool SahMatMi { get; set; } = false;
        public string? Notasyon { get; set; } // "e2-e4", "O-O" gibi
        public Guid? YenilenTasId { get; set; } // Rakip taş yenmesi

        public DateTime HamleTarihi { get; set; }
        public Oyun ?Oyun { get; set; }
        public Oyuncu ?Oyuncu { get; set; }
        public Tas? Tas { get; set; }
        
    }
}
