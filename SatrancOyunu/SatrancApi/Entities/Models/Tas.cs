namespace SatrancAPI.Entities.Models
{
    public class Tas
    {
        public Guid TasId { get; set; }
        public Guid OyunId { get; set; }
        public Guid OyuncuId { get; set; }
        public Renk renk { get; set; }
        public TasTuru turu { get; set; }  // Örn: "Piyon", "Kale", "At", vb.
        public int X { get; set; }

        public int Y { get; set; }
        public bool AktifMi { get; set; }
        // Navigation properties
        public Oyun ?Oyun { get; set; }
        public Oyuncu? Oyuncu { get; set; }
    }
}
