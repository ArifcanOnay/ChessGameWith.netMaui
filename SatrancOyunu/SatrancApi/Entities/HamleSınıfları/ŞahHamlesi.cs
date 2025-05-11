using SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYönleri;
using SatrancApi.Entities.HamlelerinYonleri.SahHamlesiYonleri;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class SahHamlesi : IHamle
    {
       
            public List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta)
            {
                var hamleler = new List<(int x, int y)>();

                // Şah her yönde sadece 1 kare hareket edebilir
                hamleler.AddRange(TumYonler.Hesapla(tas, tahta));

                //// Rök hamlesi için kontrol (ilave özellik)
                 if (Rok.GecerliMi(tas, tahta))
                 {
                     hamleler.AddRange(Rok.Hesapla(tas, tahta));
                 }

                return hamleler;
            }
        }

    }

