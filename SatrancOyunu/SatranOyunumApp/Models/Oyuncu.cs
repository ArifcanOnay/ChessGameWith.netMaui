using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
{
    public class Oyuncu
    {
        public Guid Id { get; set; }
        public string? isim { get; set; } // API'nizde küçük harfle
        public string? email { get; set; } // API'nizde küçük harfle
        public string? Sifre { get; set; }
        public Renk renk { get; set; } // API'nizde küçük harfle
    }
}
