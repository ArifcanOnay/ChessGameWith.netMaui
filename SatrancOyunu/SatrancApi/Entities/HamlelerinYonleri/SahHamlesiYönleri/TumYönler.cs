using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYonleri
{
    public class TumYonler
    {
        public static List<(int x, int y)> Hesapla(Tas sah, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();

            // Şah her yönde sadece 1 kare hareket edebilir
            int[] deltaX = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] deltaY = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int yeniX = sah.X + deltaX[i];
                int yeniY = sah.Y + deltaY[i];

                // Sınırlar içinde mi?
                if (yeniX >= 0 && yeniX < 8 && yeniY >= 0 && yeniY < 8)
                {
                    var hedefTas = tahta[yeniX, yeniY];

                    // Hedef kare boş veya düşman taşı mı?
                    if (hedefTas == null || hedefTas.renk != sah.renk)
                    {
                        hamleler.Add((yeniX, yeniY));
                    }
                }
            }

            return hamleler;
        }
    }
}