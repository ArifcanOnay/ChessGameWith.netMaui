using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
{
    public class Oyun
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
        public Durum? Durum { get; set; }
        public int SiraKimin { get; set; }
        public int ToplamHamleSayisi { get; set; }
        public bool SahDurumu { get; set; }
        public string? KazananOyuncu { get; set; }
        public string? BitisNedeni { get; set; }
    }
}
