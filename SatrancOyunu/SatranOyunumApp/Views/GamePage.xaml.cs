using SatranOyunumApp.Services;
using SatranOyunumApp.Models;
using System.Collections.ObjectModel; // ✅ YENİ: ObservableCollection için

namespace SatranOyunumApp.Views;

// ✅ YENİ: Oyun geçmişi için basit model sınıfı
public class OyunGecmisiItem
{
    public int OyunNo { get; set; }
    public DateTime OyunTarihi { get; set; }
    public string Kazanan { get; set; } = "";
    public string DurumIkonu { get; set; } = "";
}

public partial class GamePage : ContentPage
{
    private readonly ISatrancApiService _apiService; // 
    private Button[,] _tahtaButonlari = new Button[8, 8];
    // Mevcut field'larınızın altına ekleyin
    private Button? _secilikTas = null;           // Seçili taş butonu
    private List<(int X, int Y)> _gecerliHamleler = new();  // API'den gelen geçerli hamleler
    private bool _beyazSirasi = true;             // Sıra kontrolü
    private Guid _aktifOyunId = Guid.Empty;       // Aktif oyun ID'si
    private bool _hamleBekleniyor = false;        // API çağrısı kilidi
    private List<Tas> _tahtaTaslari = new();      // API'den gelen taş listesi

    // Renk sabitleri
    private readonly Color _normalBeyazKare = Colors.WhiteSmoke;
    private readonly Color _normalKahveKare = Colors.SaddleBrown;
    private readonly Color _seciliTasRengi = Color.FromRgb(100, 149, 237); // CornflowerBlue
    private readonly Color _gecerliHamleRengi = Color.FromRgb(144, 238, 144); // LightGreen
    private readonly Color _gecersizHamleRengi = Color.FromRgb(240, 128, 128); // LightCoral
    private bool _oyunBasladi = false;

    // Kullanıcı bilgileri
    private string _kullaniciAdi = "Misafir";
    private string _kullaniciEmail = "";
    private Guid _kullaniciId = Guid.Empty;

    // Mevcut field'lardan sonra ekle:

    // ✅ YENİ: Timer için field'lar
    private Timer? _hamleTimer;                    // Hamle zamanlayıcısı
    private TimeSpan _kalanSure = TimeSpan.FromMinutes(5); // 5 dakika
    private bool _timerCalisiyorMu = false;        // Timer durumu

    // ✅ YENİ: CollectionView için ObservableCollection
    private ObservableCollection<OyunGecmisiItem> _oyunGecmisi = new();

    public GamePage(ISatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService; // ✅ Inject edilen service'i kullan

        InitializeDefaults();
        SatrancTahtasiniOlustur();
        KullaniciBilgileriniYukle();

        // ✅ YENİ: CollectionView'i bağla ve örnek veriler yükle
        OyunGecmisiCollectionView.ItemsSource = _oyunGecmisi;
        OrnekOyunGecmisiniYukle();
    }

    // ✅ YENİ: Örnek oyun geçmişi verilerini yükle
    private void OrnekOyunGecmisiniYukle()
    {
        _oyunGecmisi.Add(new OyunGecmisiItem
        {
            OyunNo = 1,
            OyunTarihi = DateTime.Now.AddDays(-2),
            Kazanan = "Beyaz",
            DurumIkonu = "♔"
        });

        _oyunGecmisi.Add(new OyunGecmisiItem
        {
            OyunNo = 2,
            OyunTarihi = DateTime.Now.AddDays(-1),
            Kazanan = "Siyah",
            DurumIkonu = "♚"
        });
    }

    // ✅ YENİ: Oyun bittiğinde geçmişe ekle
    private void OyunGecmisineEkle(string kazanan)
    {
        var yeniOyun = new OyunGecmisiItem
        {
            OyunNo = _oyunGecmisi.Count + 1,
            OyunTarihi = DateTime.Now,
            Kazanan = kazanan,
            DurumIkonu = kazanan == "Beyaz" ? "♔" : "♚"
        };

        // En üste ekle (son oyun en üstte görünsün)
        _oyunGecmisi.Insert(0, yeniOyun);
    }

    // Sayfa her açıldığında çalışacak metod
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her açıldığında kullanıcı bilgilerini yeniden yükle
        KullaniciBilgileriniYukle();
    }

    // Kullanıcı bilgilerini yeniden yükleme
    private void KullaniciBilgileriniYukle()
    {
        try
        {
            // Preferences'dan kullanıcı bilgilerini al
            _kullaniciAdi = Preferences.Get("KullaniciAdi", "Misafir");
            _kullaniciEmail = Preferences.Get("KullaniciEmail", "");

            // Eğer kullanıcı çıkış yapmışsa varsayılan değerleri göster
            if (string.IsNullOrEmpty(_kullaniciEmail) || _kullaniciAdi == "Misafir")
            {
                KullaniciLabel.Text = "Kullanıcı: Misafir";
                EmailLabel.Text = "📧 Giriş yapılmadı";
            }
            else
            {
                // UI'yi güncelle
                KullaniciLabel.Text = $"Kullanıcı: {_kullaniciAdi}";
                EmailLabel.Text = $"📧 {_kullaniciEmail}";
            }
        }
        catch
        {
            // Hata durumunda varsayılan değerler
            KullaniciLabel.Text = "Kullanıcı: Misafir";
            EmailLabel.Text = "📧 Giriş yapılmadı";
        }
    }

    private void InitializeDefaults()
    {
        // Varsayılan değerleri ayarla
        // Varsayılan değerleri ayarla
        RenkLabel.Text = "Beyaz";
        RenkLabel.TextColor = Colors.DarkBlue;

        // 5 dakika olarak ayarla
        _kalanSure = TimeSpan.FromMinutes(5);
        HamleSuresiLabel.Text = "05:00";
    }

    private void SatrancTahtasiniOlustur()
    {
        // ChessBoard grid'ini temizle
        ChessBoard.Children.Clear();
        ChessBoard.ColumnDefinitions.Clear();
        ChessBoard.RowDefinitions.Clear();

        // 8x8 grid oluştur
        for (int i = 0; i < 8; i++)
        {
            ChessBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ChessBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        // Her kare için buton oluştur
        for (int satir = 0; satir < 8; satir++)
        {
            for (int sutun = 0; sutun < 8; sutun++)
            {
                var buton = new Button
                {
                    BackgroundColor = (satir + sutun) % 2 == 0 ? Colors.WhiteSmoke : Colors.SaddleBrown,
                    BorderWidth = 1,
                    BorderColor = Colors.Black,
                    FontSize = 40,              // 32'den 40'a çıkarıldı
                    FontAttributes = FontAttributes.Bold,  // Kalın yazı tipi
                    TextColor = Colors.Black,   // Siyah renk korundu
                    FontFamily = "Segoe UI Symbol",  // Daha iyi sembol yazı tipi
                    Padding = new Thickness(0),          // Padding'i sıfırla
                    Margin = new Thickness(0),           // Margin'i sıfırla
                    HorizontalOptions = LayoutOptions.Fill,   // Tam genişlik
                    VerticalOptions = LayoutOptions.Fill,

                    Text = "",
                    CornerRadius = 0
                };

                // Buton koordinatlarını kaydet
                buton.CommandParameter = $"{sutun},{satir}";
                buton.Clicked += OnTahtaKaresiTiklandi;

                // Grid'e ekle
                Grid.SetRow(buton, satir);
                Grid.SetColumn(buton, sutun);
                ChessBoard.Children.Add(buton);

                // Butonları dizide sakla
                _tahtaButonlari[satir, sutun] = buton;
            }
        }

        // Başlangıç taş pozisyonlarını yerleştir
        BaslangicTaslariniYerlestir();
    }

    private void BaslangicTaslariniYerlestir()
    {
        // Unicode satranç sembolleri
        string[,] baslangicPozisyonu = new string[8, 8]
        {
            // Siyah taşlar (0. satır)
            { "♜", "♞", "♝", "♛", "♚", "♝", "♞", "♜" },
            // Siyah piyonlar (1. satır)
            { "♟", "♟", "♟", "♟", "♟", "♟", "♟", "♟" },
            // Boş kareler
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            // Beyaz piyonlar (6. satır)
            { "♙", "♙", "♙", "♙", "♙", "♙", "♙", "♙" },
            // Beyaz taşlar (7. satır)
            { "♖", "♘", "♗", "♕", "♔", "♗", "♘", "♖" }
        };

        // Taş sembollerini butonlara yerleştir
        for (int satir = 0; satir < 8; satir++)
        {
            for (int sutun = 0; sutun < 8; sutun++)
            {
                _tahtaButonlari[satir, sutun].Text = baslangicPozisyonu[satir, sutun];

                // Beyaz taşları görünür yapma
                if (!string.IsNullOrEmpty(baslangicPozisyonu[satir, sutun]))
                {
                    // Siyah taşlar için koyu renk
                    if (satir <= 1)
                    {
                        _tahtaButonlari[satir, sutun].TextColor = Colors.Black;
                    }
                    // Beyaz taşlar için güncellenmiş çözüm
                    else if (satir >= 6)
                    {
                        bool beyazKare = (satir + sutun) % 2 == 0;

                        if (beyazKare)
                        {
                            // Beyaz karelerde: Koyu gri taş + açık arka plan
                            _tahtaButonlari[satir, sutun].TextColor = Color.FromRgb(60, 60, 60); // Koyu gri
                            _tahtaButonlari[satir, sutun].BackgroundColor = Color.FromRgb(250, 250, 250); // Hafif gri arka plan
                        }
                        else
                        {
                            // Kahverengi karelerde: Beyaz taş
                            _tahtaButonlari[satir, sutun].TextColor = Colors.White;
                        }

                        // Her iki durumda da güçlü gölge ekle
                        _tahtaButonlari[satir, sutun].Shadow = new Shadow
                        {
                            Brush = new SolidColorBrush(Colors.Black),
                            Offset = new Point(3, 3),
                            Radius = 5,
                            Opacity = 1.0f // Maksimum gölge
                        };
                    }
                }
            }
        }
    }

    private async void OnTahtaKaresiTiklandi(object sender, EventArgs e)
    {
        if (sender is Button buton && buton.CommandParameter != null)
        {
            if (_hamleBekleniyor) return; // API çağrısı devam ediyorsa bekle

            var koordinatlar = buton.CommandParameter.ToString().Split(',');
            int x = int.Parse(koordinatlar[0]);
            int y = int.Parse(koordinatlar[1]);

            if (_secilikTas == null)
            {
                // Taş seçme işlemi
                await TasSecmeIslemi(x, y, buton);
            }
            else
            {
                // Hamle yapma veya farklı taş seçme işlemi
                await HamleYapmaIslemi(x, y, buton);
            }
        }
    }

    private async Task TasSecmeIslemi(int x, int y, Button buton)
    {
        if (!TasSecilebilirMi(x, y))
        {
            await DisplayAlert("Uyarı", "Bu taşı seçemezsiniz!", "Tamam");
            return;
        }

        // Taşı seç
        _secilikTas = buton;
        buton.BackgroundColor = _seciliTasRengi;

        // Seçilen taşın ID'sini bul
        var secilenTas = _tahtaTaslari.FirstOrDefault(t => t.X == x && t.Y == y && t.AktifMi);
        if (secilenTas != null)
        {
            try
            {
                // API'den geçerli hamleleri al
                var gecerliHamleler = await _apiService.GecerliHamlelerGetir(_aktifOyunId, secilenTas.TasId);

                // Geçerli hamleleri kaydet ve görselleştir
                _gecerliHamleler.Clear();
                foreach (dynamic hamle in gecerliHamleler)
                {
                    int hamleX = (int)hamle.x;
                    int hamleY = (int)hamle.y;
                    _gecerliHamleler.Add((hamleX, hamleY));

                    // Geçerli hamle karelerini yeşil yap
                    _tahtaButonlari[hamleY, hamleX].BackgroundColor = _gecerliHamleRengi;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Geçerli hamleler alınırken hata: {ex.Message}", "Tamam");
            }
        }
    }

    private async void TestOyunuBaslat(object sender, EventArgs e)
    {
        await TestOyunuBaslat();
    }

    private async Task HamleYapmaIslemi(int x, int y, Button buton)
    {
        // Seçili taşın koordinatları
        var secilikKoordinatlar = _secilikTas.CommandParameter.ToString().Split(',');
        int seciliX = int.Parse(secilikKoordinatlar[0]);
        int seciliY = int.Parse(secilikKoordinatlar[1]);

        // Aynı kareye tekrar tıklandıysa seçimi kaldır
        if (x == seciliX && y == seciliY)
        {
            SeciliTasiTemizle();
            return;
        }

        // Başka bir taşa tıklandıysa yeni taş seç
        if (TasSecilebilirMi(x, y))
        {
            SeciliTasiTemizle();
            await TasSecmeIslemi(x, y, buton);
            return;
        }

        // Hamle yapılabilir mi kontrol et
        bool gecerliHamle = _gecerliHamleler.Any(h => h.X == x && h.Y == y);
        await DisplayAlert("Debug",
    $"Hedef: ({x},{y})\n" +
    $"Geçerli hamleler: {string.Join(", ", _gecerliHamleler.Select(h => $"({h.X},{h.Y})"))}\n" +
    $"Geçerli mi: {gecerliHamle}",
    "Tamam");

        if (gecerliHamle)
        {
            await HamleYap(seciliX, seciliY, x, y);
        }
        else
        {
            // Geçersiz hamle - kırmızı renk göster
            buton.BackgroundColor = _gecersizHamleRengi;
            await Task.Delay(500); // 0.5 saniye bekle
            TahtaRenkleriniSifirla();

            await DisplayAlert("Uyarı", "Bu hamle geçerli değil!", "Tamam");
            SeciliTasiTemizle();
        }
    }

    private async Task HamleYap(int baslangicX, int baslangicY, int hedefX, int hedefY)
    {
        _hamleBekleniyor = true;

        try
        {
            // Hamle yapılacak taşı bul
            var tas = _tahtaTaslari.FirstOrDefault(t => t.X == baslangicX && t.Y == baslangicY && t.AktifMi);
            if (tas == null)
            {
                await DisplayAlert("Hata", "Taş bulunamadı!", "Tamam");
                return;
            }

            // ✅ YENİ: Hedef konumda düşman şahı var mı kontrol et
            var hedefTas = _tahtaTaslari.FirstOrDefault(t => t.X == hedefX && t.Y == hedefY && t.AktifMi);
            bool sahYenildi = false;
            string yenilenSah = "";

            if (hedefTas != null)
            {
                // Şah sembollerini kontrol et
                if (hedefTas.TasSimgesi == "♔") // Beyaz şah
                {
                    sahYenildi = true;
                    yenilenSah = "Beyaz";
                }
                else if (hedefTas.TasSimgesi == "♚") // Siyah şah
                {
                    sahYenildi = true;
                    yenilenSah = "Siyah";
                }
            }

            // API'ye hamle gönder
            var sonuc = await _apiService.HamleYap(_aktifOyunId, tas.TasId, hedefX, hedefY);

            if (sonuc.Basarili)
            {
                // Başarılı hamle - tahtayı güncelle
                await TahtaTaslariniYukle();

                // ✅ ŞAH YENİLDİ Mİ KONTROL ET
                if (sahYenildi)
                {
                    TimerDurdur();
                    string kazanan = yenilenSah == "Beyaz" ? "Siyah" : "Beyaz";

                    // ✅ YENİ: Oyun geçmişine ekle
                    OyunGecmisineEkle(kazanan);

                    await DisplayAlert("🏆 OYUN BİTTİ",
                        $"Şah mat oldu! Kazanan: {kazanan}",
                        "Tamam");
                    return; // Oyun bitti, devam etme
                }

                // Sırayı değiştir (tek kişilik oyun)
                _beyazSirasi = !_beyazSirasi;

                // Sıra göstergesini güncelle
                await SiraGostergesiniGuncelle();
                RenkPickerGuncelle();
                TimerBaslat();

                // UI'ı güncelle
                BeyazOyuncuLabel.Text = _beyazSirasi ? "Beyaz: Sırada" : "Beyaz: Bekliyor";
                SiyahOyuncuLabel.Text = !_beyazSirasi ? "Siyah: Sırada" : "Siyah: Bekliyor";

                await DisplayAlert("Başarılı", "Hamle yapıldı!", "Tamam");
            }
            else
            {
                await DisplayAlert("Hata", sonuc.Mesaj ?? "Hamle yapılamadı!", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Hamle yapılırken hata: {ex.Message}", "Tamam");
        }
        finally
        {
            SeciliTasiTemizle();
            _hamleBekleniyor = false;
        }

    }

    // Test oyunu başlat
    public async Task TestOyunuBaslat()
    {
        try
        {
            await DisplayAlert("Bilgi", "Oyun başlatılıyor...,", "Tamam");

            // TEK OYUNCU oluştur - hem beyaz hem siyah için kullanılacak
            var tekOyuncu = await _apiService.OyuncuOlustur(_kullaniciAdi, _kullaniciEmail, Renk.Beyaz);

            if (tekOyuncu.Basarili)
            {
                await DisplayAlert("Bilgi", "Oyuncu oluşturuldu, oyun oluşturuluyor...", "Tamam");

                // Aynı oyuncuyu hem beyaz hem siyah olarak ata
                var oyunSonucu = await _apiService.YeniOyunOlustur(tekOyuncu.Oyuncu.Id, tekOyuncu.Oyuncu.Id);

                if (oyunSonucu.Basarili)
                {
                    _aktifOyunId = oyunSonucu.Oyun.OyunId;
                    _beyazSirasi = true;
                    _oyunBasladi = false;

                    // İlk sıra göstergesini ayarla
                    await SiraGostergesiniGuncelle();

                    await DisplayAlert("Bilgi", $"Oyun oluşturuldu! ID: {_aktifOyunId}", "Tamam");

                    // Tahtayı yükle
                    await TahtaTaslariniYukle();
                    _oyunBasladi = true;

                    await DisplayAlert("Başarılı", "Oyun Başlatıldı! Artık taşlara tıklayabilirsiniz.", "Tamam");
                    TimerBaslat();
                    RenkPickerGuncelle();
                }
                else
                {
                    await DisplayAlert("Hata", $"Oyun oluşturulamadı: {oyunSonucu.Mesaj}", "Tamam");
                }
            }
            else
            {
                await DisplayAlert("Hata", $"Oyuncu oluşturulamadı!\nHata: {tekOyuncu.Mesaj}", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Oyun başlatılamadı: {ex.Message}", "Tamam");
        }
    }

    // Çıkış yapma metodu
    private async void CikisYap(object sender, EventArgs e)
    {
        try
        {
            // Çıkış onayı al
            bool cikisOnay = await DisplayAlert("Çıkış",
                "Çıkış yapmak istediğinizden emin misiniz?",
                "Evet", "Hayır");

            if (cikisOnay)
            {
                TimerDurdur();
                // Kullanıcı bilgilerini temizle
                Preferences.Remove("KullaniciAdi");
                Preferences.Remove("KullaniciEmail");

                // Yerel değişkenleri de temizle
                _kullaniciAdi = "Misafir";
                _kullaniciEmail = "";

                // UI'yi hemen güncelle
                KullaniciLabel.Text = "Kullanıcı: Misafir";
                EmailLabel.Text = "📧 Giriş yapılmadı";

                // Başarı mesajı
                await DisplayAlert("Çıkış", "Başarıyla çıkış yapıldı!", "Tamam");

                // Login sayfasına yönlendir
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Çıkış yapılırken hata oluştu: {ex.Message}", "Tamam");
        }
    }

    // Seçili taş temizleme
    private void SeciliTasiTemizle()
    {
        if (_secilikTas != null)
        {
            // Seçili taşın rengini normale döndür
            TahtaRenkleriniSifirla();
            _secilikTas = null;
        }
        _gecerliHamleler.Clear();
    }

    // Tahta renklerini sıfırlama
    private void TahtaRenkleriniSifirla()
    {
        for (int satir = 0; satir < 8; satir++)
        {
            for (int sutun = 0; sutun < 8; sutun++)
            {
                // Normal satranç tahtası renkleri
                _tahtaButonlari[satir, sutun].BackgroundColor =
                    (satir + sutun) % 2 == 0 ? _normalBeyazKare : _normalKahveKare;
            }
        }
    }

    // Taş seçilebilir mi kontrolü
    private bool TasSecilebilirMi(int x, int y)
    {
        // API'den gelen taş listesinde bu pozisyonda taş var mı?
        var tas = _tahtaTaslari.FirstOrDefault(t => t.X == x && t.Y == y && t.AktifMi);
        if (tas == null) return false;

        // TEK KİŞİLİK OYUN: Sadece sıradaki rengin taşlarını seç
        bool tasBeyaz = tas.renk == Renk.Beyaz;

        // Beyaz sırası ise sadece beyaz taşlar, siyah sırası ise sadece siyah taşlar
        return (_beyazSirasi && tasBeyaz) || (!_beyazSirasi && !tasBeyaz);
    }

    // Mevcut taşları API'den yükle
    private async Task TahtaTaslariniYukle()
    {
        if (_aktifOyunId == Guid.Empty) return;

        try
        {
            _tahtaTaslari = await _apiService.OyunTaslariniGetir(_aktifOyunId);
            TahtayiGuncelle();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Taşlar yüklenirken hata: {ex.Message}", "Tamam");
        }
    }

    // Tahtayı API verilerine göre güncelle
    private void TahtayiGuncelle()
    {
        // Önce tüm kareleri temizle
        for (int satir = 0; satir < 8; satir++)
        {
            for (int sutun = 0; sutun < 8; sutun++)
            {
                _tahtaButonlari[satir, sutun].Text = "";
            }
        }

        // API'den gelen taşları yerleştir
        foreach (var tas in _tahtaTaslari.Where(t => t.AktifMi))
        {
            if (tas.X >= 0 && tas.X < 8 && tas.Y >= 0 && tas.Y < 8)
            {
                _tahtaButonlari[tas.Y, tas.X].Text = tas.TasSimgesi;

                // Taş renklendirme (mevcut kodunuz)
                if (tas.renk == Renk.Siyah)
                {
                    _tahtaButonlari[tas.Y, tas.X].TextColor = Colors.Black;
                }
                else
                {
                    bool beyazKare = (tas.Y + tas.X) % 2 == 0;
                    if (beyazKare)
                    {
                        _tahtaButonlari[tas.Y, tas.X].TextColor = Color.FromRgb(60, 60, 60);
                        _tahtaButonlari[tas.Y, tas.X].BackgroundColor = Color.FromRgb(250, 250, 250);
                    }
                    else
                    {
                        _tahtaButonlari[tas.Y, tas.X].TextColor = Colors.White;
                    }
                }
            }
        }
    }

    private async Task SiraGostergesiniGuncelle()
    {
        if (_beyazSirasi)
        {
            SiraLabel.Text = "🎯 Sıra: Beyaz";
            SiraLabel.TextColor = Colors.DarkBlue;
        }
        else
        {
            SiraLabel.Text = "🎯 Sıra: Siyah";
            SiraLabel.TextColor = Colors.DarkRed;
        }
        // ✅ YENİ: Sıra değiştiğinde şah durumunu kontrol et
        if (_oyunBasladi)
        {
            await SahDurumuKontrolEt();
        }
    }
    // ✅ YENİ: Timer metotları
    private void TimerBaslat()
    {
        // Eski timer'ı durdur
        TimerDurdur();

        // 5 dakika sıfırla
        _kalanSure = TimeSpan.FromMinutes(5);
        _timerCalisiyorMu = true;

        // Süreyi hemen göster
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HamleSuresiniGuncelle();
        });

        // Timer'ı başlat (1 saniye sonra başlasın, sonra her saniye)
        _hamleTimer = new Timer(TimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        System.Diagnostics.Debug.WriteLine("Timer başlatıldı - 5:00");
    }

    private void TimerDurdur()
    {
        _timerCalisiyorMu = false;
        _hamleTimer?.Dispose();
        _hamleTimer = null;
    }

    private void TimerTick(object? state)
    {
        if (!_timerCalisiyorMu || _kalanSure.TotalSeconds <= 0)
            return;

        _kalanSure = _kalanSure.Subtract(TimeSpan.FromSeconds(1));

        // UI thread'de güncelle
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HamleSuresiniGuncelle();

            // Süre doldu mu kontrol et
            if (_kalanSure.TotalSeconds <= 0)
            {
                SureDoldu();
            }
        });
    }

    private void HamleSuresiniGuncelle()
    {
        try
        {
            // Label'ı güncelle
            if (_kalanSure.TotalSeconds >= 0)
            {
                string dakika = ((int)_kalanSure.TotalMinutes).ToString("00");
                string saniye = _kalanSure.Seconds.ToString("00");
                HamleSuresiLabel.Text = $"{dakika}:{saniye}";

                // Süre azaldıkça renk değiştir
                if (_kalanSure.TotalSeconds <= 30)
                    HamleSuresiLabel.TextColor = Colors.Red;
                else if (_kalanSure.TotalSeconds <= 60)
                    HamleSuresiLabel.TextColor = Colors.Orange;
                else
                    HamleSuresiLabel.TextColor = Colors.DarkRed;
            }

            // Debug için console'a yazdır
            System.Diagnostics.Debug.WriteLine($"Kalan süre: {_kalanSure:mm\\:ss}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Timer güncelleme hatası: {ex.Message}");
        }
    }

    private async void SureDoldu()
    {
        TimerDurdur();

        // Sırayı değiştir
        _beyazSirasi = !_beyazSirasi;
        await SiraGostergesiniGuncelle();
        RenkPickerGuncelle(); // Yeni metot

        await DisplayAlert("Süre Doldu!",
            $"Hamle süresi doldu! Sıra {(_beyazSirasi ? "Beyaz" : "Siyah")}'a geçti.",
            "Tamam");

        // Yeni sıra için timer başlat
        TimerBaslat();
    }

    // ✅ YENİ: Renk picker'ı güncelle
    private void RenkPickerGuncelle()
    {
        if (_beyazSirasi)
        {
            RenkLabel.Text = "Beyaz";
            RenkLabel.TextColor = Colors.DarkBlue;
        }
        else
        {
            RenkLabel.Text = "Siyah";
            RenkLabel.TextColor = Colors.DarkRed;
        }
    }
    private async Task SahDurumuKontrolEt()
    {
        if (_aktifOyunId == Guid.Empty) return;

        try
        {
            var durum = await _apiService.OyunDurumuGetir(_aktifOyunId);

            if (durum.Basarili)
            {
                // ✅ 1. API'DEN ŞAH MAT KONTROLÜ
                if (durum.BeyazSahMat || durum.SiyahSahMat)
                {
                    TimerDurdur();
                    string kazanan = durum.BeyazSahMat ? "Siyah" : "Beyaz";
                    await DisplayAlert("🏆 OYUN BİTTİ",
                        $"Şah mat oldu! Kazanan: {kazanan}",
                        "Tamam");
                    return;
                }

                // ✅ 2. MANUEL ŞAH KONTROLÜ (Şahlar tahtada var mı?)
                var beyazSah = _tahtaTaslari.FirstOrDefault(t => t.TasSimgesi == "♔" && t.AktifMi);
                var siyahSah = _tahtaTaslari.FirstOrDefault(t => t.TasSimgesi == "♚" && t.AktifMi);

                if (beyazSah == null)
                {
                    TimerDurdur();
                    await DisplayAlert("🏆 OYUN BİTTİ",
                        "Şah mat oldu! Kazanan: Siyah",
                        "Tamam");
                    return;
                }

                if (siyahSah == null)
                {
                    TimerDurdur();
                    await DisplayAlert("🏆 OYUN BİTTİ",
                        "Şah mat oldu! Kazanan: Beyaz",
                        "Tamam");
                    return;
                }

                // ✅ 3. SADECE TEHDİT UYARISI (Oyun devam eder)
                bool aktifOyuncuSahTehdit = (_beyazSirasi && durum.BeyazSahTehditAltinda) ||
                                           (!_beyazSirasi && durum.SiyahSahTehditAltinda);

                if (aktifOyuncuSahTehdit)
                {
                    await DisplayAlert("⚠️ DİKKAT",
                        "Şahınız tehdit altında olabilir, lütfen kontrol ediniz.",
                        "Anladım");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Şah kontrolü hatası: {ex.Message}");
        }
    }

}