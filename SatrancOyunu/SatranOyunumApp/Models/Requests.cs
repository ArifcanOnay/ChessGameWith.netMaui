using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
{
    public class OyunOlusturRequest
    {
        public Guid BeyazOyuncuId { get; set; }
        public Guid SiyahOyuncuId { get; set; }
    }

    public class HamleRequest
    {
        public Guid TasId { get; set; }
        public int HedefX { get; set; }
        public int HedefY { get; set; }
    }

    public class OyuncuOlusturRequest
    {
        public string isim { get; set; } // API'nizde küçük harfle
        public string email { get; set; } // API'nizde küçük harfle
        public Renk renk { get; set; } // API'nizde küçük harfle
    }

    public class PiyonTerfiRequest
    {
        public Guid PiyonId { get; set; }
        public TasTuru YeniTasTuru { get; set; }
    }
}
