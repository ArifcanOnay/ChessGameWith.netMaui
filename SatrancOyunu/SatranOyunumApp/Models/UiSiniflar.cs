using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
{
    public class AlinanTas
    {
        public string TasSimgesi { get; set; }
        public TasTuru TasTuru { get; set; }
        public Renk Renk { get; set; }
    }

    public class HamleGecmisiItem
    {
        public int HamleNo { get; set; }
        public string Notasyon { get; set; }
        public string Zaman { get; set; }
    }
}
