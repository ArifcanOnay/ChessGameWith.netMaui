using SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYonleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class SahHamlesi : IHamle
    {
        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            // ✅ Normal şah hamleleri (1 kare her yöne)
            hamleler.AddRange(TumYonler.Hesapla(tas, tahta));

            // ✅ ROK HAMLELERİ EKLE
            hamleler.AddRange(Rok.Hesapla(tas, tahta));

            return hamleler;
        }
    }
}
