// Models/Tas.cs - Satranç taşı bilgilerini tutuyor
using System.Text.Json.Serialization;

namespace ChessGameMaui.Models
{
    public class Tas
    {
        [JsonPropertyName("tasId")]
        public Guid TasId { get; set; }

        [JsonPropertyName("oyunId")]
        public Guid OyunId { get; set; }

        [JsonPropertyName("oyuncuId")]
        public Guid OyuncuId { get; set; }

        [JsonPropertyName("renk")]
        public Renk Renk { get; set; }

        [JsonPropertyName("turu")]
        public TasTuru Turu { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("aktifMi")]
        public bool AktifMi { get; set; }

        [JsonPropertyName("hicHareketEtmediMi")]
        public bool HicHareketEtmediMi { get; set; }
    }
}