using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
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
        public bool RokMu { get; set; }
        public bool EnPassantMi { get; set; }
        public TasTuru? TerfiEdildigiTas { get; set; }
        public bool SahMi { get; set; }
        public bool SahMatMi { get; set; }
        public string? Notasyon { get; set; }
        public Guid? YenilenTasId { get; set; }
        public DateTime HamleTarihi { get; set; }

        // UI için ek property'ler
        public int HamleNo => (int)Math.Ceiling((HamleTarihi.Ticks % 1000) / 100.0);
        public string Zaman => HamleTarihi.ToString("HH:mm:ss");
    }
}
