using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.FilhamlesiYönleri
{
    public class CaprazHareket
    {
        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            int x = tas.X;
            int y = tas.Y;

            // 4 çapraz yön: sağ-üst, sağ-alt, sol-üst, sol-alt
            int[] xYonleri = { 1, 1, -1, -1 };
            int[] yYonleri = { 1, -1, 1, -1 };

            for (int yon = 0; yon < 4; yon++)
            {
                int i = 1;
                int yeniX = x + (i * xYonleri[yon]);
                int yeniY = y + (i * yYonleri[yon]);

                // Tahta sınırları içinde mi ve bu yönde hareket edilebilir mi?
                if (yeniX >= 0 && yeniX < 8 && yeniY >= 0 && yeniY < 8)
                {
                    // Boş kare veya rakip taş varsa hareket edilebilir
                    if (tahta[yeniX, yeniY] == null || tahta[yeniX, yeniY].renk != tas.renk)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            // 4 çapraz yön: sağ-üst, sağ-alt, sol-üst, sol-alt
            int[] xYonleri = { 1, 1, -1, -1 };
            int[] yYonleri = { 1, -1, 1, -1 };

            for (int yon = 0; yon < 4; yon++)
            {
                int i = 1;
                while (true)
                {
                    int yeniX = x + (i * xYonleri[yon]);
                    int yeniY = y + (i * yYonleri[yon]);

                    // Tahta sınırları dışına çıkarsa döngüyü sonlandır
                    if (yeniX < 0 || yeniX >= 8 || yeniY < 0 || yeniY >= 8)
                        break;

                    // Boş kare ise hamle listesine ekle ve devam et
                    if (tahta[yeniX, yeniY] == null)
                    {
                        hamleler.Add((yeniX, yeniY));
                        i++;
                    }
                    // Rakip taş ise hamle listesine ekle ve bu yönde hareket etmeyi durdur
                    else if (tahta[yeniX, yeniY].renk != tas.renk)
                    {
                        hamleler.Add((yeniX, yeniY));
                        break;
                    }
                    // Kendi taşı ise bu yönde hareket etmeyi durdur
                    else
                    {
                        break;
                    }
                }
            }

            return hamleler;
        }
    }
}
