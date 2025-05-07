using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public interface IHamle
    {
        //Her bir taş için hamleler olacagından dolayı bir tane hamle interfaci tanımlamak gerekli
        //Hamleleri de nasıl tanımlamak gerekir
        List<(int x, int y)> getGecerliHamleler(Tas tas, Tas[,] tahta);

    }
}
