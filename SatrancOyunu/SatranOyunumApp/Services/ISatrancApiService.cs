using SatranOyunumApp.Models;

namespace SatranOyunumApp.Services
{
   public interface ISatrancApiService
    {
        // Bağlantı Testi
        Task<bool> TestConnection();
        // Piyon terfi metodu
        Task<HamleSonucu> PiyonTerfiEt(Guid oyunId, Guid piyonId, TasTuru yeniTasTuru);

        // Oyuncu İşlemleri - Method signature'ları düzeltildi
        Task<OyuncuOlusturSonucu> OyuncuOlustur(string isim, string email, Renk renk);
        Task<KullaniciKayitSonucu> KullaniciKaydet(string isim, string email, string sifre); // Dönüş tipi düzeltildi
        Task<LoginSonucu> Login(string email, string sifre);
        Task<List<Oyuncu>> TumOyunculariGetir();
      
        Task<bool> SifreDegistirAsync(string email, string eskiSifre, string yeniSifre, string yeniSifreTekrar);

        // Oyun İşlemleri
        Task<OyunOlusturSonucu> YeniOyunOlustur(Guid beyazOyuncuId, Guid siyahOyuncuId);
        Task<List<Oyun>> TumOyunlariGetir();
        Task<Oyun?> OyunGetir(Guid oyunId);

        // Taş İşlemleri
        Task<List<Tas>> OyunTaslariniGetir(Guid oyunId);
        Task<HamleSonucu> HamleYap(Guid oyunId, Guid tasId, int hedefX, int hedefY);
        Task<List<dynamic>> GecerliHamlelerGetir(Guid oyunId, Guid tasId);

        // Oyun Durumu
        Task<OyunDurumSonucu> OyunDurumuGetir(Guid oyunId);
        Task<string> TestOyunDurumuEndpoint(Guid oyunId);

       
        Task<List<Hamle>> HamleGecmisiniGetir(Guid oyunId);
        Task<OyuncuOlusturSonucu> YeniOyuncuOlustur(string isim, string email, int renk);
    }
}