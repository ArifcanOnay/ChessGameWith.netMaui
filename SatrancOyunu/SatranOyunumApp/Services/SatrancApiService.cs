using System.Text.Json;
using System.Text;
using System.Net.Http;
using SatranOyunumApp.Models;

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

        // ========== MEVCUT KULLANICI YÖNETİMİ METHODLARI ==========
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
                    return false;
                }
            }
            catch (Exception ex)
            {
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
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    // *** GÜVENLİ PROPERTY KONTROLÜ ***
                    string kullaniciAdi = email.Split('@')[0]; // Default olarak email'den al

                    // API'den KullaniciAdi varsa onu kullan
                    if (loginResponse.TryGetProperty("KullaniciAdi", out JsonElement kullaniciAdiElement))
                    {
                        kullaniciAdi = kullaniciAdiElement.GetString() ?? kullaniciAdi;
                    }

                    return new LoginSonucu
                    {
                        Basarili = true,
                        Mesaj = "Giriş başarılı",
                        KullaniciAdi = kullaniciAdi
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

        // ========== YENİ SATRANÇ OYUNU METHODLARI ==========

        // 1. Tüm oyunları getir
        public async Task<List<Oyun>> TumOyunlariGetir()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/oyunlar");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Oyun>>(json, GetJsonOptions()) ?? new List<Oyun>();
                }
                return new List<Oyun>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyunlar getirilirken hata: {ex.Message}");
            }
        }

        // 2. ID'ye göre oyunu getir
        public async Task<Oyun?> OyunGetir(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/oyunlar/{oyunId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Oyun>(json, GetJsonOptions());
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyun getirilirken hata: {ex.Message}");
            }
        }

        // 3. Yeni oyun oluştur
        public async Task<OyunOlusturSonucu> YeniOyunOlustur(Guid beyazOyuncuId, Guid siyahOyuncuId)
        {
            try
            {
                var request = new
                {
                    BeyazOyuncuId = beyazOyuncuId,
                    SiyahOyuncuId = siyahOyuncuId
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/oyunlar", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var oyun = JsonSerializer.Deserialize<Oyun>(responseJson, GetJsonOptions());
                    return new OyunOlusturSonucu { Basarili = true, Oyun = oyun, Mesaj = "Oyun başarıyla oluşturuldu" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new OyunOlusturSonucu { Basarili = false, Mesaj = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new OyunOlusturSonucu { Basarili = false, Mesaj = $"Oyun oluşturulurken hata: {ex.Message}" };
            }
        }

        // 4. Bir oyundaki tüm taşları getir
        public async Task<List<Tas>> OyunTaslariniGetir(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/oyunlar/{oyunId}/taslar");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Tas>>(json, GetJsonOptions()) ?? new List<Tas>();
                }
                return new List<Tas>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Taşlar getirilirken hata: {ex.Message}");
            }
        }

        // 5. Bir taşın geçerli hamlelerini getir
        public async Task<List<object>> GecerliHamlelerGetir(Guid oyunId, Guid tasId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/oyunlar/{oyunId}/taslar/{tasId}/gecerli-hamleler");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<object>>(json, GetJsonOptions()) ?? new List<object>();
                }
                return new List<object>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Geçerli hamleler getirilirken hata: {ex.Message}");
            }
        }

        // 6. Hamle yap
        public async Task<HamleSonucu> HamleYap(Guid oyunId, Guid tasId, int hedefX, int hedefY)
        {
            try
            {
                var request = new
                {
                    TasId = tasId,
                    HedefX = hedefX,
                    HedefY = hedefY
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/oyunlar/{oyunId}/hamleler", content);

                if (response.IsSuccessStatusCode)
                {
                    return new HamleSonucu { Basarili = true, Mesaj = "Hamle başarıyla yapıldı" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new HamleSonucu { Basarili = false, Mesaj = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new HamleSonucu { Basarili = false, Mesaj = $"Hamle yapılırken hata: {ex.Message}" };
            }
        }

        // 7. Hamleleri getir
        public async Task<List<Hamle>> HamleGecmisiniGetir(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/oyunlar/{oyunId}/hamleler");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Hamle>>(json, GetJsonOptions()) ?? new List<Hamle>();
                }
                return new List<Hamle>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Hamle geçmişi getirilirken hata: {ex.Message}");
            }
        }

        // 8. Yeni oyuncu oluştur
        public async Task<OyuncuOlusturSonucu> YeniOyuncuOlustur(string isim, string email, int renk)
        {
            try
            {
                var request = new
                {
                    isim = isim,
                    email = email,
                    renk = renk
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/oyuncular", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var oyuncu = JsonSerializer.Deserialize<Oyuncu>(responseJson, GetJsonOptions());
                    return new OyuncuOlusturSonucu { Basarili = true, Oyuncu = oyuncu, Mesaj = "Oyuncu başarıyla oluşturuldu" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new OyuncuOlusturSonucu { Basarili = false, Mesaj = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new OyuncuOlusturSonucu { Basarili = false, Mesaj = $"Oyuncu oluşturulurken hata: {ex.Message}" };
            }
        }

        // 9. Tüm oyuncuları getir
        public async Task<List<Oyuncu>> TumOyunculariGetir()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/oyuncular");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Oyuncu>>(json, GetJsonOptions()) ?? new List<Oyuncu>();
                }
                return new List<Oyuncu>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyuncular getirilirken hata: {ex.Message}");
            }
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
    }

    // ========== MEVCUT SONUÇ SINIFLARI ==========
    public class LoginSonucu
    {
        public bool Basarili { get; set; }
        public string? Mesaj { get; set; }
        public string? KullaniciAdi { get; set; }
    }

    public class KullaniciKayitSonucu
    {
        public bool Basarili { get; set; }
        public string? Mesaj { get; set; }
    }

    // ========== YENİ SATRANÇ SONUÇ SINIFLARI ==========
    public class OyunOlusturSonucu
    {
        public bool Basarili { get; set; }
        public string? Mesaj { get; set; }
        public Oyun? Oyun { get; set; }
    }

    public class HamleSonucu
    {
        public bool Basarili { get; set; }
        public string? Mesaj { get; set; }
    }

    public class OyuncuOlusturSonucu
    {
        public bool Basarili { get; set; }
        public string? Mesaj { get; set; }
        public Oyuncu? Oyuncu { get; set; }
    }
}