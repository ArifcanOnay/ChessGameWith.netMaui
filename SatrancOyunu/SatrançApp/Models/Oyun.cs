// Models/Oyun.cs - API'deki Oyun entity'si ile aynı yapıda MAUI için model
using System.Text.Json.Serialization;

namespace ChessGameMaui.Models
{
    public class Oyun
    {
        [JsonPropertyName("oyunId")]
        public Guid OyunId { get; set; }

        [JsonPropertyName("beyazOyuncuId")]
        public Guid BeyazOyuncuId { get; set; }

        [JsonPropertyName("siyahOyuncuId")]
        public Guid SiyahOyuncuId { get; set; }

        [JsonPropertyName("baslangicTarihi")]
        public DateTime BaslangicTarihi { get; set; }

        [JsonPropertyName("bitisTarihi")]
        public DateTime? BitisTarihi { get; set; }

        [JsonPropertyName("beyazSkor")]
        public int BeyazSkor { get; set; }

        [JsonPropertyName("siyahSkor")]
        public int SiyahSkor { get; set; }

        [JsonPropertyName("durum")]
        public Durum Durum { get; set; }

        [JsonPropertyName("beyazOyuncu")]
        public Oyuncu? BeyazOyuncu { get; set; }

        [JsonPropertyName("siyahOyuncu")]
        public Oyuncu? SiyahOyuncu { get; set; }

        [JsonPropertyName("siraKimin")]
        public int SiraKimin { get; set; }

        [JsonPropertyName("toplamHamleSayisi")]
        public int ToplamHamleSayisi { get; set; }

        [JsonPropertyName("sahDurumu")]
        public bool SahDurumu { get; set; }

        [JsonPropertyName("kazananOyuncu")]
        public string? KazananOyuncu { get; set; }

        [JsonPropertyName("bitisNedeni")]
        public string? BitisNedeni { get; set; }
    }
}