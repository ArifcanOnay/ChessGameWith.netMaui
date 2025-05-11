using SatrancApi.Entities.HamlelerinYonleri.KaleYönleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class KaleHamlesi : IHamle
    {

        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            if (DuzHareket.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(DuzHareket.Hesapla(tas, tahta));
            }

            return hamleler;
        }
    }
}
