using SatrancAPI.Entities.Models;

namespace SatrancAPI.Entities.HamleSınıfları
{
    public class HamleFactory
    {
        public static IHamle HamleSinifiGetir(TasTuru tasTuru)
        {
            return tasTuru switch
            {
                TasTuru.Piyon => new PiyonHamlesi(),
                TasTuru.Kale => new KaleHamlesi(),
                TasTuru.At => new AtHamlesi(),
                TasTuru.Fil => new FilHamlesi(),
                TasTuru.Vezir => new VezirHamlesi(),
                TasTuru.Şah => new SahHamlesi(),
                _ => throw new ArgumentException("Geçersiz taş türü")
            };
        }
    }
}