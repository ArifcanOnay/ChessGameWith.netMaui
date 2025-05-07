using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri
{
    public class CaprazYeme
    {
        public static bool gecerliMi(Tas tas, Tas[,] tahta)
        {
            
            int x = tas.X; int y = tas.Y;
            if (tas.renk == Renk.Beyaz)
            {
                return x > 0 && y > 0 && tahta[x - 1, y - 1]?.renk == Renk.Siyah ||
                       x > 0 && y < 7 && tahta[x - 1, y + 1]?.renk == Renk.Siyah;
            }
            else // Siyah
            {
                return x < 7 && y > 0 && tahta[x + 1, y - 1]?.renk == Renk.Beyaz ||
                       x < 7 && y < 7 && tahta[x + 1, y + 1]?.renk == Renk.Beyaz;
            }
        }
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            if (tas.renk == Renk.Beyaz)
            {
                if (x > 0 && y > 0 && tahta[x - 1, y - 1]?.renk == Renk.Siyah)
                    hamleler.Add((x - 1, y - 1));
                if (x > 0 && y < 7 && tahta[x - 1, y + 1]?.renk == Renk.Siyah)
                    hamleler.Add((x - 1, y + 1));
            }
            else // Siyah
            {
                if (x < 7 && y > 0 && tahta[x + 1, y - 1]?.renk == Renk.Beyaz)
                    hamleler.Add((x + 1, y - 1));
                if (x < 7 && y < 7 && tahta[x + 1, y + 1]?.renk == Renk.Beyaz)
                    hamleler.Add((x + 1, y + 1));
            }

            return hamleler;
        }

    }
}
