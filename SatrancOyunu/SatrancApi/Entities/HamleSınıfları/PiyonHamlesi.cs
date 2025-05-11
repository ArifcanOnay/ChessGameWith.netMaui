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
            hamleler.AddRange(İleriHamle.Hesapla(tas, tahta));
            if (CaprazYeme.gecerliMi(tas, tahta))
            {
                hamleler.AddRange(CaprazYeme.Hesapla(tas, tahta));
               
            }
            

             if(İlkHamledeCiftAdim.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(İlkHamledeCiftAdim.Hesapla(tas, tahta));
                
            }
            if (PiyonTerfi.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(PiyonTerfi.Hesapla(tas, tahta));
            }
          
            return hamleler;




        }
    }
}
