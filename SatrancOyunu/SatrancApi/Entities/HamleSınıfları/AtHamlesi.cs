using SatrancApi.Entities.HamlelerinYonleri.AticinLHareketi;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class AtHamlesi : IHamle
    {
        public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
        {
        
            var hamleler = new List<(int x, int y)>();

            // Önce geçerli hamle var mı kontrol ediyoruz
            if (Lhareketi.GecerliMi(tas, tahta))
            {
                // Geçerli hamleler varsa onları hesaplıyoruz
                hamleler.AddRange(Lhareketi.Hesapla(tas, tahta));
            }

            return hamleler;
        
    }
    }
}
