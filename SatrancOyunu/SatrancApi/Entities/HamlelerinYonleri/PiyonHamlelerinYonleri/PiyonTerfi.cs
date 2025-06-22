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

            
            if (tas.renk == Renk.Beyaz)
            {
                return x== 0; 
            }
            else 
            {
                return x== 7; 
            }
        }

        // Piyonun terfi ettiği yeri hesaplar ve geçerli terfi hamlesini ekler.
        public static List<(int x, int y)> Hesapla(Tas tas, Tas[,] tahta)
        {
            List<(int x, int y)> hamleler = new List<(int x, int y)>();
            int x = tas.X;
            int y = tas.Y;

            // Beyaz piyonlar için terfi işlemi
            if (tas.renk == Renk.Beyaz && x==1)
            {
                if (tahta[0, y] == null)
                {
                    hamleler.Add((0, y)); // Terfi alanına gidebilir
                }
            }
            else if(tas.renk==Renk.Siyah && x==6) // Siyah piyonlar için terfi işlemi
            {
                // İlerideki kare boşsa terfi edilebilir
                if (tahta[7, y] == null)
                {
                    hamleler.Add((7, y)); // Terfi alanına gidebilir
                }
            }

            return hamleler;
        }

    }
}
