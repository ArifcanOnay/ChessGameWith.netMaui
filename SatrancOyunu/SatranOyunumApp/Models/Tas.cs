using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatranOyunumApp.Models
{

    public class Tas
    {
        public Guid TasId { get; set; }
        public Guid OyunId { get; set; }
        public Guid OyuncuId { get; set; }
        public Renk renk { get; set; } // API'nizde küçük harfle
        public TasTuru turu { get; set; } // API'nizde küçük harfle
        public int X { get; set; }
        public int Y { get; set; }
        public bool AktifMi { get; set; }
        public bool HicHareketEtmediMi { get; set; }
        public DateTime? SonHareketTarihi { get; set; }
        public int? EnPassantTuru { get; set; }

        // UI için ek property'ler
        public string TasSimgesi => GetTasSimgesi();
        public string RenkAdi => renk == Renk.Beyaz ? "Beyaz" : "Siyah";

        private string GetTasSimgesi()
        {
            // Unicode chess symbols
            var symbols = new Dictionary<(Renk, TasTuru), string>
            {
                { (Renk.Beyaz, TasTuru.Şah), "♔" },
                { (Renk.Beyaz, TasTuru.Vezir), "♕" },
                { (Renk.Beyaz, TasTuru.Kale), "♖" },
                { (Renk.Beyaz, TasTuru.Fil), "♗" },
                { (Renk.Beyaz, TasTuru.At), "♘" },
                { (Renk.Beyaz, TasTuru.Piyon), "♙" },
                { (Renk.Siyah, TasTuru.Şah), "♚" },
                { (Renk.Siyah, TasTuru.Vezir), "♛" },
                { (Renk.Siyah, TasTuru.Kale), "♜" },
                { (Renk.Siyah, TasTuru.Fil), "♝" },
                { (Renk.Siyah, TasTuru.At), "♞" },
                { (Renk.Siyah, TasTuru.Piyon), "♟" }
            };

            return symbols.TryGetValue((renk, turu), out var symbol) ? symbol : "?";
        }
    }

}
