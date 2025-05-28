// Models/Enums.cs - API'deki enumlar ile uyumlu
using System.Text.Json.Serialization;

namespace ChessGameMaui.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Renk
    {
        Siyah = 0,
        Beyaz = 1
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TasTuru
    {
        Piyon = 0,
        Kale = 1,
        At = 2,
        Fil = 3,
        Vezir = 4,
        Şah = 5
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Durum
    {
        Bitiyor = 0,
        Oynaniyor = 1,
        Berabere = 2
    }
}