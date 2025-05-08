using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.AticinLHareketi
{
    public class Lhareketi
    {
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            // Atın L şeklinde hareketi için tüm 8 olası yön
            int[] xYonleri = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] yYonleri = { 1, 2, 2, 1, -1, -2, -2, -1 };

            for (int i = 0; i < 8; i++)
            {
                int hedefX = tas.X + xYonleri[i];
                int hedefY = tas.Y + yYonleri[i];

                // Tahta sınırları içinde mi kontrol et
                if (hedefX >= 0 && hedefX < 8 && hedefY >= 0 && hedefY < 8)
                {
                    // Hedef kare boş veya rakip taş içeriyor mu?
                    if (tahta[hedefX, hedefY] == null || tahta[hedefX, hedefY].renk != tas.renk)
                    {
                        hamleler.Add((hedefX, hedefY));
                    }
                }
            }

            return hamleler;
        }

        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            // Atın her zaman hareket etme potansiyeli vardır, 
            // sadece tahtadaki konumuna ve diğer taşlara bağlı
            int[] xYonleri = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] yYonleri = { 1, 2, 2, 1, -1, -2, -2, -1 };

            for (int i = 0; i < 8; i++)
            {
                int hedefX = tas.X + xYonleri[i];
                int hedefY = tas.Y + yYonleri[i];

                if (hedefX >= 0 && hedefX < 8 && hedefY >= 0 && hedefY < 8)
                {
                    if (tahta[hedefX, hedefY] == null || tahta[hedefX, hedefY].renk != tas.renk)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

