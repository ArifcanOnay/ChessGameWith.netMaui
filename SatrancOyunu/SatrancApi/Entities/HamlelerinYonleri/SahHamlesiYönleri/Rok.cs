using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYonleri
{
    public class Rok
    {
        public static bool GecerliMi(Tas sah, Tas[,] tahta)
        {
            // Şah hiç hareket etmedi mi kontrolü (veritabanında bir flag tutabilirsiniz)
            // Örnek: if (sah.HicHareketEtmediMi == false) return false;

            // Şah tehdit altında mı kontrolü

            // Vezir tarafı rok için kontrol
            if (KisaRokGecerliMi(sah, tahta))
                return true;

            // Şah tarafı rok için kontrol
            if (UzunRokGecerliMi(sah, tahta))
                return true;

            return false;
        }

        private static bool KisaRokGecerliMi(Tas sah, Tas[,] tahta)
        {
            int y = sah.Y;
            // Sağdaki kale kontrolü
            Tas kale = tahta[7, y];

            // Kale var mı, doğru renkte mi ve hiç hareket etmemiş mi kontrolü
            if (kale == null || kale.turu != TasTuru.Kale || kale.renk != sah.renk)
                return false;
            // Örnek: if (kale.HicHareketEtmediMi == false) return false;

            // Aralarında taş var mı kontrolü
            for (int x = sah.X + 1; x < 7; x++)
            {
                if (tahta[x, y] != null)
                    return false;
            }

            // Şahın geçeceği kareler tehdit altında mı kontrolü
            // Bu kontrolü şimdilik basit tutuyoruz

            return true;
        }

        private static bool UzunRokGecerliMi(Tas sah, Tas[,] tahta)
        {
            // Benzer mantıkta soldaki kale için kontroller
            // ...
            return false; // Örnek
        }

        public static List<(int x, int y)> Hesapla(Tas sah, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();

            if (KisaRokGecerliMi(sah, tahta))
            {
                // Kısa rok (şahın sağa 2 kare gitmesi)
                hamleler.Add((sah.X + 2, sah.Y));
            }

            if (UzunRokGecerliMi(sah, tahta))
            {
                // Uzun rok (şahın sola 2 kare gitmesi)
                hamleler.Add((sah.X - 2, sah.Y));
            }

            return hamleler;
        }
    }
}