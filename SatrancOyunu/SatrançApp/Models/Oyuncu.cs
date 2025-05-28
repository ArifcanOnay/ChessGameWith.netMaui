// Models/Oyuncu.cs - Oyuncu bilgilerini tutuyor
using System.Text.Json.Serialization;

namespace ChessGameMaui.Models
{
    public class Oyuncu
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("isim")]
        public string? Isim { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("renk")]
        public Renk Renk { get; set; }
    }
}