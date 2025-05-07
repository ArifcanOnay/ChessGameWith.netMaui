using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamlelerinYonleri.PiyonHamlelerinYonleri
{
    public class PiyonTerfi
    {
        // Piyonun terfi edebilmesi için geçerli olup olmadığını kontrol eder.
        public static bool GecerliMi(Tas tas, Tas[,] tahta)
        {
            int x = tas.X;
            int y = tas.Y;

            // Beyaz piyon için: Y ekseninde 7. sıraya ulaşması gerekir
            if (tas.renk == Renk.Beyaz)
            {
                return y == 6; // Beyaz piyon 6. sırada (y = 6) olmalı
            }
            else // Siyah piyon için: Y ekseninde 0. sıraya ulaşması gerekir
            {
                return y == 1; // Siyah piyon 1. sırada (y = 1) olmalı
            }
        }

        // Piyonun terfi ettiği yeri hesaplar ve geçerli terfi hamlesini ekler.
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            // Beyaz piyonlar için terfi işlemi
            if (tas.renk == Renk.Beyaz)
            {
                // Beyaz piyon 7. sıraya ulaşırsa (terfi alanı)
                if (y == 6)
                {
                    hamleler.Add((x, 7)); // Terfi alanına gidebilir
                }
            }
            else // Siyah piyonlar için terfi işlemi
            {
                // Siyah piyon 0. sıraya ulaşırsa (terfi alanı)
                if (y == 1)
                {
                    hamleler.Add((x, 0)); // Terfi alanına gidebilir
                }
            }

            return hamleler;
        }

    }
}
