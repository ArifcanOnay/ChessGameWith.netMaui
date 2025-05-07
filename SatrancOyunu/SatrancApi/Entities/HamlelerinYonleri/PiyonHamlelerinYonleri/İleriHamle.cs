using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri
{
    public class İleriHamle
    {
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            var hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            if (tas.renk == Renk.Beyaz && x > 0 && tahta[x - 1, y] == null)
                hamleler.Add((x - 1, y));
            else if (tas.renk == Renk.Siyah && x < 7 && tahta[x + 1, y] == null)
                hamleler.Add((x + 1, y));

            return hamleler;
        }

    }
}
