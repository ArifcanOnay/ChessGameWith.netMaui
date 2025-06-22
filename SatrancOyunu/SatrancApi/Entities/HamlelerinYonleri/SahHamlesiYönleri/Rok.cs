using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYonleri
{
    public class Rok
    {
        public static List<(int x, int y)> Hesapla(Tas sah, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();

            //  ŞART: Şah hiç hareket etmemiş mi?
            if (!sah.HicHareketEtmediMi)
                return hamleler;

            //  KISA ROK (Sağa - Y koordinatında +2)
            if (KisaRokGecerliMi(sah, tahta))
            {
                hamleler.Add((sah.X, sah.Y + 2)); // Şah sağa 2 kare
            }

            //  UZUN ROK (Sola - Y koordinatında -2)  
            if (UzunRokGecerliMi(sah, tahta))
            {
                hamleler.Add((sah.X, sah.Y - 2)); // Şah sola 2 kare
            }

            return hamleler;
        }

        //  KISA ROK (Kral tarafı - sağ kale)
        private static bool KisaRokGecerliMi(Tas sah, Tas[,] tahta)
        {
            int sahX = sah.X;
            int sahY = sah.Y;

            // Sağdaki kaleyi kontrol et (Y+3 konumunda)
            if (sahY + 3 >= 8) return false; // Sınır kontrolü

            var kale = tahta[sahX, sahY + 3]; // Sağdaki kale
            if (kale == null || kale.turu != TasTuru.Kale || kale.renk != sah.renk)
                return false;

            // Kale hiç hareket etmemiş mi?
            if (!kale.HicHareketEtmediMi)
                return false;

            // Aralarında taş var mı? (Y+1 ve Y+2 boş olmalı)
            if (tahta[sahX, sahY + 1] != null || tahta[sahX, sahY + 2] != null)
                return false;

            return true;
        }

        //  UZUN ROK (Vezir tarafı - sol kale)
        private static bool UzunRokGecerliMi(Tas sah, Tas[,] tahta)
        {
            int sahX = sah.X;
            int sahY = sah.Y;

            // Soldaki kaleyi kontrol et (Y-4 konumunda)
            if (sahY - 4 < 0) return false; // Sınır kontrolü

            var kale = tahta[sahX, sahY - 4]; // Soldaki kale
            if (kale == null || kale.turu != TasTuru.Kale || kale.renk != sah.renk)
                return false;

            // Kale hiç hareket etmemiş mi?
            if (!kale.HicHareketEtmediMi)
                return false;

            // Aralarında taş var mı? (Y-1, Y-2, Y-3 boş olmalı)
            if (tahta[sahX, sahY - 1] != null ||
                tahta[sahX, sahY - 2] != null ||
                tahta[sahX, sahY - 3] != null)
                return false;

            return true;
        }

        //  Rok geçerli mi (eski metod adı için uyumluluk)
        public static bool GecerliMi(Tas sah, Tas[,] tahta)
        {
            // Bu metod eskiden kullanılıyordu, şimdi true döndürüyor
            // Çünkü asıl kontrol Hesapla() metodunda yapılıyor
            return sah.turu == TasTuru.Şah && sah.HicHareketEtmediMi;
        }
    }
}