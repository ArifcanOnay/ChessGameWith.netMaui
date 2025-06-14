using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace SatranOyunumApp.Services
{
    public class SatrancApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7003";

        public SatrancApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/oyunlar");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<KullaniciKayitSonucu> KullaniciKaydet(string kullaniciAdi, string email, string sifre)
        {
            try
            {
                var request = new
                {
                    KullaniciAdi = kullaniciAdi,
                    Email = email,
                    Sifre = sifre
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/kullanicilar/kayit", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new KullaniciKayitSonucu { Basarili = true, Mesaj = "Kullanıcı başarıyla oluşturuldu" };
                }
                else
                {
                    if (responseContent.Contains("Bu email adresi ile kayıtlı kullanıcı zaten mevcut"))
                    {
                        return new KullaniciKayitSonucu { Basarili = false, Mesaj = "Bu email adresi ile kayıtlı kullanıcı zaten mevcut" };
                    }
                    else
                    {
                        return new KullaniciKayitSonucu { Basarili = false, Mesaj = "Kayıt sırasında hata oluştu" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new KullaniciKayitSonucu { Basarili = false, Mesaj = $"Bağlantı hatası: {ex.Message}" };
            }
        }
        public async Task<bool> SifreDegistirAsync(string email, string eskiSifre, string yeniSifre, string yeniSifreTekrar)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    EskiSifre = eskiSifre,
                    YeniSifre = yeniSifre,
                    YeniSifreTekrar = yeniSifreTekrar
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/kullanicilar/sifre-degistir", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Hata mesajını loglayabilir veya gösterebilirsiniz
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                return false;
            }
        }
        public async Task<LoginSonucu> Login(string email, string sifre)
        {
            try
            {
                var loginData = new
                {
                    Email = email,
                    Sifre = sifre
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/kullanicilar/login", content);

                if (response.IsSuccessStatusCode)
                {
                    return new LoginSonucu
                    {
                        Basarili = true,
                        Mesaj = "Giriş başarılı"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        var mesaj = errorResult.GetProperty("Mesaj").GetString();

                        return new LoginSonucu
                        {
                            Basarili = false,
                            Mesaj = mesaj ?? "Giriş başarısız"
                        };
                    }
                    catch
                    {
                        return new LoginSonucu
                        {
                            Basarili = false,
                            Mesaj = errorContent
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new LoginSonucu
                {
                    Basarili = false,
                    Mesaj = "Bağlantı hatası: " + ex.Message
                };
            }
        }
    }

}
public class LoginSonucu
{
    public bool Basarili { get; set; }
    public string ?Mesaj { get; set; }
}

public class KullaniciKayitSonucu
    {
        public bool Basarili { get; set; }
        public string ?Mesaj { get; set; }
    }
