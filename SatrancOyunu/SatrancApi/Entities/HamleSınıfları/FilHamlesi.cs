using SatrancApi.Entities.HamlelerinYonleri.FilhamlesiYönleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class FilHamlesi : IHamle
    {
        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            // Fil için sadece çapraz hareket geçerlidir
            if (CaprazHareket.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(CaprazHareket.Hesapla(tas, tahta));
            }

            return hamleler;
        }
    }
}
