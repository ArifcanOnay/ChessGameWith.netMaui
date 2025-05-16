namespace SatrancAPI.Entities.Models
{
    public class Hamle
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
        public DateTime HamleTarihi { get; set; }
        public Oyun ?Oyun { get; set; }
        public Oyuncu ?Oyuncu { get; set; }
        public Tas? Tas { get; set; }
        
    }
}
