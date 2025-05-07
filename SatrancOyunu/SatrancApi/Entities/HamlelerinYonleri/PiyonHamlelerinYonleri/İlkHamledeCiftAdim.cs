using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri
{
    public class İlkHamledeCiftAdim
    {
        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            int x = tas.X;
            int y = tas.Y;

            if (tas.renk == Renk.Beyaz && x == 6)
            {
                return tahta[5, y] == null && tahta[4, y] == null;
            }
            else if (tas.renk == Renk.Siyah && x == 1)
            {
                return tahta[2, y] == null && tahta[3, y] == null;
            }

            return false;
        }
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            if (tas.renk == Renk.Beyaz && x == 6 && tahta[5, y] == null && tahta[4, y] == null)
            {
                hamleler.Add((4, y));
            }
            else if (tas.renk == Renk.Siyah && x == 1 && tahta[2, y] == null && tahta[3, y] == null)
            {
                hamleler.Add((3, y));
            }

            return hamleler;
        }
    }
}
