using SatranOyunumApp.Services;
using SatranOyunumApp.Models;

namespace SatranOyunumApp.Views;

public partial class GamePage : ContentPage
{
    private readonly SatrancApiService _apiService;
    private Button[,] _tahtaButonlari = new Button[8, 8];

    // YENİ: Kullanıcı bilgileri
    private string _kullaniciAdi = "Misafir";
    private string _kullaniciEmail = "";
    private Guid _kullaniciId = Guid.Empty;

    public GamePage()
    {
        InitializeComponent();
        _apiService = new SatrancApiService(new HttpClient());
        InitializeDefaults();
        SatrancTahtasiniOlustur();

        // YENİ: Sayfa yüklenirken kullanıcı bilgilerini kontrol et
        KullaniciBilgileriniYukle();
    }

    // YENİ EKLENEN: Sayfa her açıldığında çalışacak metod
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her açıldığında kullanıcı bilgilerini yeniden yükle
        KullaniciBilgileriniYukle();
    }

    // GÜNCELLENDİ: Kullanıcı bilgilerini yeniden yükleme
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
        RenkPicker.SelectedIndex = 0; // Beyaz
        HamleSuresiPicker.Time = new TimeSpan(0, 5, 0); // 5 dakika
    }

    private async void YeniOyunBaslat(object sender, EventArgs e)
    {
        try
        {
            // GÜNCELLENDİ: Kullanıcı giriş yapmış mı kontrol et
            if (string.IsNullOrEmpty(_kullaniciEmail) || _kullaniciAdi == "Misafir")
            {
                await DisplayAlert("Uyarı", "Oyun oynamak için giriş yapmalısınız!", "Tamam");
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            await DisplayAlert("Bilgi", $"Yeni oyun başlatılıyor...\nOyuncu: {_kullaniciAdi}", "Tamam");

            // Seçilen ayarları kontrol et
            string secilenRenk = RenkPicker.SelectedItem?.ToString() ?? "Beyaz";
            bool sesAcik = SesCheckBox.IsChecked;
            TimeSpan hamleSuresi = HamleSuresiPicker.Time;

            await DisplayAlert("Ayarlar",
                $"Renk: {secilenRenk}\nSes: {(sesAcik ? "Açık" : "Kapalı")}\nHamle Süresi: {hamleSuresi:mm\\:ss}\nOyuncu: {_kullaniciAdi}",
                "Tamam");

            // API bağlantısını test et
            await TestApiConnection();

            // YENİ: API'ye yeni oyun oluşturma isteği gönder
            await YeniOyunOlustur(secilenRenk);

        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
        }
    }

    // YENİ: API'ye yeni oyun oluşturma
    private async Task YeniOyunOlustur(string secilenRenk)
    {
        try
        {
            // Geçici olarak random ID'ler kullan (sonra gerçek kullanıcı ID'leri gelecek)
            Guid beyazOyuncuId = Guid.NewGuid();
            Guid siyahOyuncuId = Guid.NewGuid();
            var oyunSonucu = await _apiService.YeniOyunOlustur(beyazOyuncuId, siyahOyuncuId);

            if (oyunSonucu.Basarili)
            {
                await DisplayAlert("Başarılı",
                    $"Yeni oyun oluşturuldu!  Oyuncu: {_kullaniciAdi}",
                    "Tamam");
            }
            else
            {
                await DisplayAlert("Hata", $"Oyun oluşturulamadı: {oyunSonucu.Mesaj}", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("API Hatası", $"Oyun oluşturma hatası: {ex.Message}", "Tamam");
        }
    }

    private async Task TestApiConnection()
    {
        try
        {
            // API bağlantısını test et
            bool baglanti = await _apiService.TestConnection();

            if (baglanti)
            {
                await DisplayAlert("Başarılı", "API bağlantısı başarılı!", "Tamam");
            }
            else
            {
                await DisplayAlert("Hata", "API'ye bağlanılamadı. Lütfen API'nin çalıştığından emin olun.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", $"API bağlantı hatası: {ex.Message}", "Tamam");
        }
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
                buton.CommandParameter = new { X = sutun, Y = satir };
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

                // *** GÜNCELLENDİ: Beyaz taşları görünür yapma ***
                if (!string.IsNullOrEmpty(baslangicPozisyonu[satir, sutun]))
                {
                    // Siyah taşlar için koyu renk
                    if (satir <= 1)
                    {
                        _tahtaButonlari[satir, sutun].TextColor = Colors.Black;
                    }
                    // *** BEYAZ TAŞLAR İÇİN GÜNCELLENMİŞ ÇÖZÜM ***
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
            var koordinat = (dynamic)buton.CommandParameter;

            // YENİ: Hamle bilgisine kullanıcı bilgilerini ekle
            await DisplayAlert("Kare Tıklandı",
                $"Oyuncu: {_kullaniciAdi}\nKoordinat: X:{koordinat.X}, Y:{koordinat.Y}\nEmail: {_kullaniciEmail}",
                "Tamam");
        }
    }

    // GÜNCELLENDİ: Çıkış yapma metodu
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
}