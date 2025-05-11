using SatrancAPI.Entities.Models;

namespace SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYönleri
{
   
        public class TumYonler
        {
            public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
            {
                List<(int x, int y)> hamleler = new List<(int x, int y)>();
                int x = tas.X;
                int y = tas.Y;

                // 8 yön: sağ, sol, yukarı, aşağı, sağ-üst, sağ-alt, sol-üst, sol-alt
                int[] xYonleri = { 1, -1, 0, 0, 1, 1, -1, -1 };
                int[] yYonleri = { 0, 0, 1, -1, 1, -1, 1, -1 };

                for (int yon = 0; yon < 8; yon++)
                {
                    int yeniX = x + xYonleri[yon];
                    int yeniY = y + yYonleri[yon];

                    if (yeniX >= 0 && yeniX < 8 && yeniY >= 0 && yeniY < 8)
                    {
                        if (tahta[yeniX, yeniY] == null || tahta[yeniX, yeniY].renk != tas.renk)
                        {
                            // Şah kontrolü yapılmalı - şahın tehdit altında olduğu kareye gidemez
                            // Basitlik için şimdilik bu kontrolü atlıyoruz
                            hamleler.Add((yeniX, yeniY));
                        }
                    }
                }

                return hamleler;
            }
        }
    }

