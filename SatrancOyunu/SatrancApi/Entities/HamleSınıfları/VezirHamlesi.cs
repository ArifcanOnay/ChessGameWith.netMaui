using SatrancApi.Entities.HamlelerinYonleri.FilhamlesiYönleri;
using SatrancApi.Entities.HamlelerinYonleri.KaleYönleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class VezirHamlesi : IHamle
    {

        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            // Vezir hem fil hem kale gibi hareket edebilir
            if (CaprazHareket.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(CaprazHareket.Hesapla(tas, tahta));
            }

            if (DuzHareket.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(DuzHareket.Hesapla(tas, tahta));
            }

            return hamleler;
        }
    }
}

