// Services/ApiService.cs - Ana API iletişim servisi
using System.Text;
using System.Text.Json;
using ChessGameMaui.Models;

namespace ChessGameMaui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7041"; // API'nizin URL'i

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Tüm oyunları getir
        public async Task<List<Oyun>> GetOyunlarAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyunlar");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Oyun>>(json, _jsonOptions) ?? new List<Oyun>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyunlar getirilemedi: {ex.Message}");
            }
        }

        // Belirli bir oyunu getir
        public async Task<Oyun?> GetOyunAsync(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyunlar/{oyunId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Oyun>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyun getirilemedi: {ex.Message}");
            }
        }

        // Yeni oyun oluştur
        public async Task<Oyun> YeniOyunOlusturAsync(Guid beyazOyuncuId, Guid siyahOyuncuId)
        {
            try
            {
                var request = new { BeyazOyuncuId = beyazOyuncuId, SiyahOyuncuId = siyahOyuncuId };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/oyunlar", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Oyun>(responseJson, _jsonOptions)!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyun oluşturulamadı: {ex.Message}");
            }
        }

        // Oyundaki taşları getir
        public async Task<List<Tas>> GetOyunTaslarAsync(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyunlar/{oyunId}/taslar");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Tas>>(json, _jsonOptions) ?? new List<Tas>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Taşlar getirilemedi: {ex.Message}");
            }
        }

        // Hamle yap
        public async Task<bool> HamleYapAsync(Guid oyunId, Guid tasId, int hedefX, int hedefY)
        {
            try
            {
                var request = new { TasId = tasId, HedefX = hedefX, HedefY = hedefY };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/oyunlar/{oyunId}/hamleler", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Hamle yapılamadı: {ex.Message}");
            }
        }

        // Geçerli hamleleri getir
        public async Task<List<(int X, int Y)>> GetGecerliHamlelerAsync(Guid oyunId, Guid tasId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyunlar/{oyunId}/taslar/{tasId}/gecerli-hamleler");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<(int, int)>>(json, _jsonOptions) ?? new List<(int, int)>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Geçerli hamleler alınamadı: {ex.Message}");
            }
        }

        // Tüm oyuncuları getir
        public async Task<List<Oyuncu>> GetOyuncularAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyuncular");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Oyuncu>>(json, _jsonOptions) ?? new List<Oyuncu>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyuncular getirilemedi: {ex.Message}");
            }
        }

        // Yeni oyuncu oluştur
        public async Task<Oyuncu> YeniOyuncuOlusturAsync(string isim, string email, Renk renk)
        {
            try
            {
                var request = new { isim = isim, email = email, renk = renk };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/oyuncular", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Oyuncu>(responseJson, _jsonOptions)!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyuncu oluşturulamadı: {ex.Message}");
            }
        }

        // Oyun durumunu getir
        public async Task<object> GetOyunDurumAsync(Guid oyunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/oyunlar/{oyunId}/durum");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions)!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Oyun durumu alınamadı: {ex.Message}");
            }
        }
    }
}