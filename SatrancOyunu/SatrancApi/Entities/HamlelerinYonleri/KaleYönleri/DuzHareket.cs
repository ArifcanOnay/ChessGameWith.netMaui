using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.KaleYönleri
{
    public class DuzHareket
    {
        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            int x = tas.X;
            int y = tas.Y;

            // 4 düz yön: sağ, sol, yukarı, aşağı
            int[] xYonleri = { 1, -1, 0, 0 };
            int[] yYonleri = { 0, 0, 1, -1 };

            for (int yon = 0; yon < 4; yon++)
            {
                int i = 1;
                int yeniX = x + (i * xYonleri[yon]);
                int yeniY = y + (i * yYonleri[yon]);

                if (yeniX >= 0 && yeniX < 8 && yeniY >= 0 && yeniY < 8)
                {
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

            // 4 düz yön: sağ, sol, yukarı, aşağı
            int[] xYonleri = { 1, -1, 0, 0 };
            int[] yYonleri = { 0, 0, 1, -1 };

            for (int yon = 0; yon < 4; yon++)
            {
                int i = 1;
                while (true)
                {
                    int yeniX = x + (i * xYonleri[yon]);
                    int yeniY = y + (i * yYonleri[yon]);

                    if (yeniX < 0 || yeniX >= 8 || yeniY < 0 || yeniY >= 8)
                        break;

                    if (tahta[yeniX, yeniY] == null)
                    {
                        hamleler.Add((yeniX, yeniY));
                        i++;
                    }
                    else if (tahta[yeniX, yeniY].renk != tas.renk)
                    {
                        hamleler.Add((yeniX, yeniY));
                        break;
                    }
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
