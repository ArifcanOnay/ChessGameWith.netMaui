using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class PiyonHamlesi : IHamle
    {
        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();
            if (CaprazYeme.gecerliMi(tas, tahta))
            {
                hamleler.AddRange(CaprazYeme.Hesapla(tas, tahta));
                return hamleler;
            }

            if (İlkHamledeCiftAdim.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(İlkHamledeCiftAdim.Hesapla(tas, tahta));
                return hamleler;
            }
            if (PiyonTerfi.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(PiyonTerfi.Hesapla(tas, tahta));
                return hamleler;

            }
            


            hamleler.AddRange(İleriHamle.Hesapla(tas, tahta));
            return hamleler;
        }
    }
}
