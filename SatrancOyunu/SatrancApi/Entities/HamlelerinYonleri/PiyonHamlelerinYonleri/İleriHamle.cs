using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri
{
    public class İleriHamle
    {
        // Piyonun ileriye doğru hamle yapıp yapamayacağını kontrol eder.
        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            int x = tas.X;
            int y = tas.Y;

            //  Koordinat sistemi ve yön kontrolü
            if (tas.renk == Renk.Beyaz)
            {
                // Beyaz piyon yukarı gider (X ekseni azalır: 6→5→4→...)
                int hedefX = x - 1;
                return hedefX >= 0 && tahta[hedefX, y] == null;
            }
            else // Siyah piyon
            {
                // Siyah piyon aşağı gider (X ekseni artar: 1→2→3→...)
                int hedefX = x + 1;
                return hedefX <= 7 && tahta[hedefX, y] == null;
            }
        }

        // Piyonun ileriye doğru gidebileceği hamleleri hesaplar.
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            //  Sadece 1 adım ileri hamle
            if (tas.renk == Renk.Beyaz)
            {
                // Beyaz piyon yukarı gider (X ekseni azalır)
                int hedefX = x - 1;
                if (hedefX >= 0 && tahta[hedefX, y] == null)
                {
                    hamleler.Add((hedefX, y));
                }
            }
            else // Siyah piyon
            {
                // Siyah piyon aşağı gider (X ekseni artar)
                int hedefX = x + 1;
                if (hedefX <= 7 && tahta[hedefX, y] == null)
                {
                    hamleler.Add((hedefX, y));
                }
            }

            return hamleler;
        }
    }
}