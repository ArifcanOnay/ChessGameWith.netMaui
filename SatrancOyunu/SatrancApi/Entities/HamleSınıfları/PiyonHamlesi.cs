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

            // 1️⃣ İleri tek adım hamle (her zaman önce kontrol et)
            hamleler.AddRange(İleriHamle.Hesapla(tas, tahta));

            // 2️⃣ İlk hamledeki çift adım (sadece ilk hamlede ve önde boşluk varsa)
            if (İlkHamledeCiftAdim.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(İlkHamledeCiftAdim.Hesapla(tas, tahta));
            }

            // 3️⃣ Çapraz yeme (düşman taş varsa)
            if (CaprazYeme.gecerliMi(tas, tahta))
            {
                hamleler.AddRange(CaprazYeme.Hesapla(tas, tahta));
            }

            // 4️⃣ Piyon terfi (son sıraya yakınsa)
            if (PiyonTerfi.GecerliMi(tas, tahta))
            {
                hamleler.AddRange(PiyonTerfi.Hesapla(tas, tahta));
            }

            return hamleler;
        }
    }
}

