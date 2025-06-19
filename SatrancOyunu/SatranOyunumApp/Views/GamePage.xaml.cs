using SatranOyunumApp.Services;
using SatranOyunumApp.Models;
using System.Collections.ObjectModel; 
#nullable disable 

namespace SatranOyunumApp.Views;




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
    private List<Tas> _tahtaTaslari = new();
    // ✅ YENİ: Koordinat sistemi field'ları
    private Label[] _ustKoordinatlar = new Label[8];   // A-H
    private Label[] _solKoordinatlar = new Label[8];   // 1-8
    private bool _tahtaDonmus = false;                 // Tahta döndü mü?

    // ✅ YENİ: Koordinat hesaplama
    private string[] _sutunHarfleri = { "A", "B", "C", "D", "E", "F", "G", "H" };
    private string[] _satirNumaralari = { "8", "7", "6", "5", "4", "3", "2", "1" };// API'den gelen taş listesi

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

    // Timer için field'lar
    private Timer? _hamleTimer;                    // Hamle zamanlayıcısı
    private TimeSpan _kalanSure = TimeSpan.FromMinutes(5); // 5 dakika
    private bool _timerCalisiyorMu = false;        // Timer durumu

    //  CollectionView için ObservableCollection
    private ObservableCollection<OyunGecmisiItem> _oyunGecmisi = new();
    private bool _sesAktif = true; // CheckBox durumu

    public GamePage(ISatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService; // Inject edilen service'i kullanmak için

        InitializeDefaults();
        SatrancTahtasiniOlustur();
        KullaniciBilgileriniYukle();

       
        OyunGecmisiCollectionView.ItemsSource = _oyunGecmisi;
        //  CheckBox event'ini bağla
        SesCheckBox.CheckedChanged += OnSesAyariDegisti;

    }

    

    //  Oyun bittiğinde geçmişe ekle
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
    private async void KullaniciBilgileriniYukle()
    {
          try
    {
        // Preferences'dan kullanıcı bilgilerini al
        _kullaniciAdi = Preferences.Get("KullaniciAdi", "Misafir");
        _kullaniciEmail = Preferences.Get("KullaniciEmail", "");

        //  Eğer oturum yoksa login'e yönlendir
        if (string.IsNullOrEmpty(_kullaniciEmail) || _kullaniciAdi == "Misafir")
        {
            KullaniciLabel.Text = "Kullanıcı: Misafir";
            EmailLabel.Text = "📧 Giriş yapılmadı";
            
            //Oturum yoksa login sayfasına yönlendir
            await DisplayAlert("Giriş Gerekli", 
                "Oyun oynamak için giriş yapmanız gerekiyor.", 
                "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
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
        // Hata durumunda login'e yönlendir
        KullaniciLabel.Text = "Kullanıcı: Misafir";
        EmailLabel.Text = "📧 Giriş yapılmadı";
        await Shell.Current.GoToAsync("//LoginPage");
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
        // ✅ YENİ: Koordinat grid'lerini temizle
        TopCoordinatesGrid.Children.Clear();
        LeftCoordinatesGrid.Children.Clear();

        // 8x8 grid oluştur
        for (int i = 0; i < 8; i++)
        {
            ChessBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ChessBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        // ✅ YENİ: Koordinat labelları oluştur
        KoordinatlariOlustur();

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
            if (!_oyunBasladi)
            {
                await DisplayAlert("Oyun Bitti",
                    "Oyun sona erdi! Yeni oyun başlatmak için 'Oyunu Başlat' butonuna tıklayın.",
                    "Tamam");
                return; // Hamle yapılmasını engelle
            }
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
            
            await SesCalat("invalid_selection");
            await DisplayAlert("Uyarı", "Bu taşı seçemezsiniz!", "Tamam");
            return;
        }
        await SesCalat("chess_move");

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
    //    await DisplayAlert("Debug",
    //$"Hedef: ({x},{y})\n" +
    //$"Geçerli hamleler: {string.Join(", ", _gecerliHamleler.Select(h => $"({h.X},{h.Y})"))}\n" +
    //$"Geçerli mi: {gecerliHamle}",
    //"Tamam");

        if (gecerliHamle)
        {
            var secilenTas = _tahtaTaslari.FirstOrDefault(t => t.X == seciliX && t.Y == seciliY && t.AktifMi);

            // Eğer şah hareket ediyorsa, güvenlik kontrolü yap
            if (secilenTas != null && secilenTas.TasSimgesi == "♔" || secilenTas.TasSimgesi == "♚")
            {
                bool güvenliMi = await SahHamleGuvenligi(seciliX, seciliY, x, y, secilenTas.renk);

                if (!güvenliMi)
                {
                    // ✅ KIRMIZI RENK + UYARI
                    buton.BackgroundColor = _gecersizHamleRengi; // Kırmızı renk
                    await SesCalat("invalid_selection"); // Hata sesi

                    await Task.Delay(800); // 0.8 saniye bekle
                    TahtaRenkleriniSifirla();

                    await DisplayAlert("⚠️ GEÇERSİZ HAMLE",
                        "Şahınızı tehlikeye atabilecek bu hamleyi yapamazsınız!\n\n" +
                        "Bu kare düşman taşları tarafından kontrol edilmektedir.",
                        "Anladım");

                    SeciliTasiTemizle();
                    return;
                }
            }
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
        string baslangicKare = KareAdiniGetir(baslangicX, baslangicY);
        string hedefKare = KareAdiniGetir(hedefX, hedefY);
        string hamleNotasyonu = $"{baslangicKare} → {hedefKare}";

        try
        {
            // Hamle yapılacak taşı bul
            var tas = _tahtaTaslari.FirstOrDefault(t => t.X == baslangicX && t.Y == baslangicY && t.AktifMi);
            if (tas == null)
            {
                await DisplayAlert("Hata", "Taş bulunamadı!", "Tamam");
                return;
            }
            //  PİYON TERFİ KONTROLÜ
            bool piyonTerfiGereklimi = false;
            if (tas.turu == TasTuru.Piyon)
            {
                // Beyaz piyon 0. sıraya, siyah piyon 7. sıraya ulaşırsa terfi
                piyonTerfiGereklimi = (tas.renk == Renk.Beyaz && hedefX == 0) ||
                                     (tas.renk == Renk.Siyah && hedefX == 7);
            }

            // Hedef konumda düşman şahı var mı kontrol et
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
                // ✅ YENİ: ROK HAMLESİ KONTROLÜ VE SESİ
                bool rokHamlesiMi = tas.turu == TasTuru.Şah && Math.Abs(hedefY - baslangicY) == 2;

                if (rokHamlesiMi)
                {
                    string rokTuru = hedefY > baslangicY ? "Kısa Rok" : "Uzun Rok";
                    await SesCalat("chess_castling");
                    await DisplayAlert("🏰 ROK BAŞARILI",
                        $"{rokTuru} başarıyla tamamlandı!",
                        "Harika!");
                }





                //  PİYON TERFİ İŞLEMİ
                if (piyonTerfiGereklimi)
                {
                    var terfiTuru = await PiyonTerfiSecimi();
                    if (terfiTuru != null)
                    {
                        var terfiSonucu = await _apiService.PiyonTerfiEt(_aktifOyunId, tas.TasId, terfiTuru.Value);
                        if (terfiSonucu.Basarili)
                        {
                            await SesCalat("chess_capture"); // Terfi sesi
                            await DisplayAlert("🎉 PİYON TERFİ",
                                $"Piyon başarıyla {TasTuruDisplayAdi(terfiTuru.Value)}'e terfi etti!",
                                "Harika!");
                        }
                    }
                }
                // Başarılı hamle - tahtayı güncelle
                await TahtaTaslariniYukle();

                // ŞAH YENİLDİ Mİ KONTROL ET
                if (sahYenildi)
                {
                    _oyunBasladi = false;
                    TimerDurdur();
                    string kazanan = yenilenSah == "Beyaz" ? "Siyah" : "Beyaz";

                    // YENİ: Oyun geçmişine ekle
                    OyunGecmisineEkle(kazanan);
                    await SesCalat("chess_checkmate");

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
                _oyunBasladi = false;
                _aktifOyunId = Guid.Empty;
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
        //  Sıra değiştiğinde şah durumunu kontrol et
        if (_oyunBasladi)
        {
            await SahDurumuKontrolEt();
        }
    }
    //  Timer metotları
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

    //  Renk picker'ı güncelle
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
        if (_aktifOyunId == Guid.Empty || !_oyunBasladi) return;

        try
        {
            var durum = await _apiService.OyunDurumuGetir(_aktifOyunId);

            if (durum.Basarili)
            {
                // ✅ YENİ: API'DEN ŞAH-MAT KONTROLÜ
                if (durum.BeyazSahMat || durum.SiyahSahMat)
                {
                    _oyunBasladi = false;
                    TimerDurdur();

                    string kazanan = durum.BeyazSahMat ? "Siyah" : "Beyaz";
                    string kaybeden = durum.BeyazSahMat ? "Beyaz" : "Siyah";

                    // Oyun geçmişine ekle
                    OyunGecmisineEkle(kazanan);

                    // Şah-mat sesi çal
                    await SesCalat("chess_checkmate");

                    // ✅ GELİŞTİRİLMİŞ ŞAH-MAT MESAJI
                    await DisplayAlert("🏆 OYUN BİTTİ - ŞAH MAT!",
                        $"🔥 {kaybeden} şah mat oldu!\n" +
                        $"👑 Kazanan: {kazanan}\n\n" +
                        $"🎯 {kaybeden} şahının kaçacak yeri kalmadı!\n" +
                        $"⚔️ Hiçbir hamle şahı kurtaramaz!",
                        "🎉 Tebrikler!");

                    return;
                }

                // ✅ MANUEL ŞAH KONTROLÜ (Şahlar tahtada var mı?)
                var beyazSah = _tahtaTaslari.FirstOrDefault(t => t.TasSimgesi == "♔" && t.AktifMi);
                var siyahSah = _tahtaTaslari.FirstOrDefault(t => t.TasSimgesi == "♚" && t.AktifMi);

                if (beyazSah == null)
                {
                    _oyunBasladi = false;
                    TimerDurdur();
                    OyunGecmisineEkle("Siyah");
                    await SesCalat("chess_checkmate");
                    await DisplayAlert("🏆 OYUN BİTTİ",
                        "Beyaz şah alındı! Kazanan: Siyah",
                        "Tamam");
                    return;
                }

                if (siyahSah == null)
                {
                    _oyunBasladi = false;
                    TimerDurdur();
                    OyunGecmisineEkle("Beyaz");
                    await SesCalat("chess_checkmate");
                    await DisplayAlert("🏆 OYUN BİTTİ",
                        "Siyah şah alındı! Kazanan: Beyaz",
                        "Tamam");
                    return;
                }

                // ✅ ŞAH TEHDİT UYARISI
                if (_oyunBasladi)
                {
                    if (_beyazSirasi && durum.BeyazSahTehditAltinda)
                    {
                        await SesCalat("chess_check");
                        await DisplayAlert("⚠️ DİKKAT - BEYAZ ŞAH",
                            "🔥 Beyaz şahınız tehdit altında!\n" +
                            "🛡️ Şahınızı koruyun veya kaçırın!",
                            "Anladım");
                    }
                    else if (!_beyazSirasi && durum.SiyahSahTehditAltinda)
                    {
                        await SesCalat("chess_check");
                        await DisplayAlert("⚠️ DİKKAT - SİYAH ŞAH",
                            "🔥 Siyah şahınız tehdit altında!\n" +
                            "🛡️ Şahınızı koruyun veya kaçırın!",
                            "Anladım");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Şah kontrolü hatası: {ex.Message}");
        }
    }
    // 
    private async Task SesCalat(string sesAdi)
    {
        try
        {
            // CheckBox kontrolü
            if (SesCheckBox?.IsChecked != true) return; //Ses kapalı veya CheckBox null, çalma

            // Gelişmiş beep sistemini çağır
            await GelismisBeep(sesAdi);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ses çalma hatası: {ex.Message}");
        }
    }

    //  Gelişmiş beep sistemi
    private async Task GelismisBeep(string sesType)
    {
        await Task.Run(() =>
        {
            try
            {
                switch (sesType)
                {
                    case "chess_move":
                        // Chess.com tarzı "tick" sesi
                        System.Console.Beep(800, 120);
                        break;

                    case "chess_capture":
                        // "Thump" sesi (taş alma)
                        System.Console.Beep(400, 100);
                        Thread.Sleep(30);
                        System.Console.Beep(900, 150);
                        break;

                    case "chess_check":
                        // "Ding" sesi (şah)
                        System.Console.Beep(1200, 200);
                        Thread.Sleep(50);
                        System.Console.Beep(1000, 100);
                        break;

                    case "chess_checkmate":
                        // "Victory" sesi
                        System.Console.Beep(600, 200);
                        Thread.Sleep(100);
                        System.Console.Beep(800, 200);
                        Thread.Sleep(100);
                        System.Console.Beep(1000, 300);
                        break;

                    case "invalid_selection":
                        // Geçersiz seçim sesi
                        System.Console.Beep(300, 200);
                        break;
                    case "chess_castling":
                        // Rok sesi - çifte beep
                        System.Console.Beep(600, 150);
                        Thread.Sleep(50);
                        System.Console.Beep(800, 150);
                        break;

                    default:
                        System.Console.Beep(800, 200);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Beep hatası: {ex.Message}");
            }
        }).ConfigureAwait(false);
    }
    private async void OnSesAyariDegisti(object sender, CheckedChangedEventArgs e)
    {
        _sesAktif = e.Value;

        if (e.Value)
        {
            // Ses açıldı - test sesi çal
            await SesCalat("chess_move");
            await DisplayAlert("Ses Ayarları", "Ses efektleri açıldı! 🔊", "Tamam");
        }
        else
        {
            await DisplayAlert("Ses Ayarları", "Ses efektleri kapatıldı. 🔇", "Tamam");
        }
    }
    // 
    private async Task<bool> SahHamleGuvenligi(int sahX, int sahY, int hedefX, int hedefY, Renk sahRengi)
    {
        try
        {
            // Geçici olarak şahı yeni pozisyona taşı (sadece memory'de)
            var geciciTahtaTaslari = new List<Tas>(_tahtaTaslari);

            // Şahı hedef pozisyona taşı
            var sah = geciciTahtaTaslari.FirstOrDefault(t => t.X == sahX && t.Y == sahY && t.AktifMi);
            if (sah != null)
            {
                sah.X = hedefX;
                sah.Y = hedefY;
            }

            // Hedef pozisyonda düşman taş varsa onu kaldır
            var hedefTas = geciciTahtaTaslari.FirstOrDefault(t => t.X == hedefX && t.Y == hedefY && t.AktifMi && t.renk != sahRengi);
            if (hedefTas != null)
            {
                hedefTas.AktifMi = false; // Geçici olarak pasif yap
            }

            // Bu pozisyonda şah tehdit altında mı kontrol et
            bool tehditAltinda = SahTehditKontrolu(hedefX, hedefY, sahRengi, geciciTahtaTaslari);

            // Geçici değişiklikleri geri al
            if (sah != null)
            {
                sah.X = sahX;
                sah.Y = sahY;
            }
            if (hedefTas != null)
            {
                hedefTas.AktifMi = true;
            }

            return !tehditAltinda; // Tehdit altında değilse güvenli
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Şah güvenlik kontrolü hatası: {ex.Message}");
            return false; // Hata durumunda güvensiz sayarak hamleyi engelle
        }
    }

    // 
    private bool SahTehditKontrolu(int sahX, int sahY, Renk sahRengi, List<Tas> tahtaTaslari)
    {
        // Düşman taşlarını kontrol et
        var dusmanTaslari = tahtaTaslari.Where(t => t.AktifMi && t.renk != sahRengi).ToList();

        foreach (var dusmanTas in dusmanTaslari)
        {
            // Bu düşman taş, şahın olacağı pozisyonu vurabilir mi?
            if (TasVurabilirMi(dusmanTas, sahX, sahY, tahtaTaslari))
            {
                return true; // Tehdit var!
            }
        }

        return false; // Güvenli
    }

    // Taş vurma kabiliyeti kontrolü
    private bool TasVurabilirMi(Tas tas, int hedefX, int hedefY, List<Tas> tahtaTaslari)
    {
        // Taş türüne göre hareket kontrolü
        switch (tas.TasSimgesi)
        {
            case "♟":
            case "♙": // Piyon
                return PiyonVurabilirMi(tas, hedefX, hedefY);

            case "♜":
            case "♖": // Kale
                return KaleVurabilirMi(tas, hedefX, hedefY, tahtaTaslari);

            case "♞":
            case "♘": // At
                return AtVurabilirMi(tas, hedefX, hedefY);

            case "♝":
            case "♗": // Fil
                return FilVurabilirMi(tas, hedefX, hedefY, tahtaTaslari);

            case "♛":
            case "♕": // Vezir
                return VezirVurabilirMi(tas, hedefX, hedefY, tahtaTaslari);

            case "♚":
            case "♔": // Şah
                return SahVurabilirMi(tas, hedefX, hedefY);

            default:
                return false;
        }
    }

    // 
    private bool PiyonVurabilirMi(Tas piyon, int hedefX, int hedefY)
    {
        int deltaX = hedefX - piyon.X;
        int deltaY = hedefY - piyon.Y;

        // Siyah piyon: sadece aşağı (deltaY = +1), Beyaz piyon: sadece yukarı (deltaY = -1)
        bool dogruYon = (piyon.renk == Renk.Siyah && deltaY == 1) || (piyon.renk == Renk.Beyaz && deltaY == -1);

        // Piyon sadece çapraz saldırır
        return dogruYon && Math.Abs(deltaX) == 1;
    }

    private bool KaleVurabilirMi(Tas kale, int hedefX, int hedefY, List<Tas> tahtaTaslari)
    {
        // Kale: düz çizgiler (yatay veya dikey)
        if (kale.X != hedefX && kale.Y != hedefY) return false;

        // Yol temiz mi kontrol et
        return YolTemizMi(kale.X, kale.Y, hedefX, hedefY, tahtaTaslari);
    }

    private bool FilVurabilirMi(Tas fil, int hedefX, int hedefY, List<Tas> tahtaTaslari)
    {
        // Fil: çapraz çizgiler
        int deltaX = Math.Abs(hedefX - fil.X);
        int deltaY = Math.Abs(hedefY - fil.Y);

        if (deltaX != deltaY) return false; // Çapraz değil

        // Yol temiz mi kontrol et
        return YolTemizMi(fil.X, fil.Y, hedefX, hedefY, tahtaTaslari);
    }

    private bool AtVurabilirMi(Tas at, int hedefX, int hedefY)
    {
        // At: L şeklinde (2+1 veya 1+2)
        int deltaX = Math.Abs(hedefX - at.X);
        int deltaY = Math.Abs(hedefY - at.Y);

        return (deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2);
    }

    private bool VezirVurabilirMi(Tas vezir, int hedefX, int hedefY, List<Tas> tahtaTaslari)
    {
        // Vezir: kale + fil hareketleri
        return KaleVurabilirMi(vezir, hedefX, hedefY, tahtaTaslari) ||
               FilVurabilirMi(vezir, hedefX, hedefY, tahtaTaslari);
    }

    private bool SahVurabilirMi(Tas sah, int hedefX, int hedefY)
    {
        // Şah: 1 kare her yöne
        int deltaX = Math.Abs(hedefX - sah.X);
        int deltaY = Math.Abs(hedefY - sah.Y);

        return deltaX <= 1 && deltaY <= 1 && (deltaX != 0 || deltaY != 0);
    }

    // 
    private bool YolTemizMi(int baslangicX, int baslangicY, int hedefX, int hedefY, List<Tas> tahtaTaslari)
    {
        int deltaX = hedefX - baslangicX;
        int deltaY = hedefY - baslangicY;

        // Yön hesapla
        int adimX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        int adimY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);

        // Yol boyunca taş var mı kontrol et
        int x = baslangicX + adimX;
        int y = baslangicY + adimY;

        while (x != hedefX || y != hedefY)
        {
            // Bu pozisyonda taş var mı?
            if (tahtaTaslari.Any(t => t.X == x && t.Y == y && t.AktifMi))
            {
                return false; // Yol kapalı
            }

            x += adimX;
            y += adimY;
        }

        return true; // Yol temiz
    }
    //Piyon terfi seçim ekranı
    private async Task<TasTuru?> PiyonTerfiSecimi()
    {
        var secim = await DisplayActionSheet(
            "🏆 PİYON TERFİ",
            "İptal",
            null,
            "♕ Vezir (En Güçlü)",
            "♖ Kale (Güçlü)",
            "♗ Fil (Çapraz)",
            "♘ At (L Şekli)"
        );

        return secim switch
        {
            "♕ Vezir (En Güçlü)" => TasTuru.Vezir,
            "♖ Kale (Güçlü)" => TasTuru.Kale,
            "♗ Fil (Çapraz)" => TasTuru.Fil,
            "♘ At (L Şekli)" => TasTuru.At,
            _ => null // İptal
        };
    }

    // ✅ YENİ: Taş türü isimlerini görüntüle
    private string TasTuruDisplayAdi(TasTuru tur)
    {
        return tur switch
        {
            TasTuru.Vezir => "Vezir",
            TasTuru.Kale => "Kale",
            TasTuru.Fil => "Fil",
            TasTuru.At => "At",
            _ => "Bilinmeyen"
        };
    }
    // GamePage.xaml.cs'ye ekle
    private void TemaPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        string secilenTema = picker.Items[picker.SelectedIndex];

        // Tema değiştirme (basit bir örnek)
        switch (secilenTema)
        {
            case "Klasik":
                this.BackgroundColor = Colors.White;
                break;
            case "Modern":
                this.BackgroundColor = Colors.Gray;
                break;
            case "Renkli":
                this.BackgroundColor = Colors.LightBlue;
                break;
        }
    }
    //  Koordinat labelları oluşturma
    private void KoordinatlariOlustur()
    {
        // ÜST KOORDINATLAR (A-H)
        for (int i = 0; i < 8; i++)
        {
            var label = new Label
            {
                Text = _tahtaDonmus ? _sutunHarfleri[7 - i] : _sutunHarfleri[i],
                FontSize = 8,  // ✅ DÜZELTME: Font küçültüldü
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromRgb(139, 69, 19), // SaddleBrown
                Padding = new Thickness(0), // ✅ DÜZELTME: Padding kaldırıldı
                Margin = new Thickness(0)   // ✅ DÜZELTME: Margin kaldırıldı
            };

            Grid.SetColumn(label, i);
            TopCoordinatesGrid.Children.Add(label);
            _ustKoordinatlar[i] = label;
        }

        // SOL KOORDINATLAR (1-8)
        for (int i = 0; i < 8; i++)
        {
            var label = new Label
            {
                Text = _tahtaDonmus ? _satirNumaralari[7 - i] : _satirNumaralari[i],
                FontSize = 8,  //  Font küçültüldü
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromRgb(139, 69, 19), // SaddleBrown
                Rotation = 0,
                Padding = new Thickness(0), //  Padding kaldırıldı
                Margin = new Thickness(0)   //  Margin kaldırıldı
            };

            Grid.SetRow(label, i);
            LeftCoordinatesGrid.Children.Add(label);
            _solKoordinatlar[i] = label;
        }
    }

    //  Koordinatları güncelleme
    private void KoordinatlariGuncelle()
    {
        // Üst koordinatları güncelle (A-H)
        for (int i = 0; i < 8; i++)
        {
            if (_ustKoordinatlar[i] != null)
            {
                _ustKoordinatlar[i].Text = _tahtaDonmus ? _sutunHarfleri[7 - i] : _sutunHarfleri[i];
            }
        }

        // Sol koordinatları güncelle (1-8)
        for (int i = 0; i < 8; i++)
        {
            if (_solKoordinatlar[i] != null)
            {
                _solKoordinatlar[i].Text = _tahtaDonmus ? _satirNumaralari[7 - i] : _satirNumaralari[i];
            }
        }
    }

    // Kare adını getir
    private string KareAdiniGetir(int sutun, int satir)
    {
        if (_tahtaDonmus)
        {
            string harf = _sutunHarfleri[7 - sutun];
            string numara = _satirNumaralari[7 - satir];
            return $"{harf}{numara}";
        }
        else
        {
            string harf = _sutunHarfleri[sutun];
            string numara = _satirNumaralari[satir];
            return $"{harf}{numara}";
        }
    }

    //  Tahta döndürme butonu
    private async void TahtaDondurBtn_Clicked(object sender, EventArgs e)
    {
        await TahtayiDondur();
    }

    //  Tahta döndürme metodu
    private async Task TahtayiDondur()
    {
        try
        {
            _tahtaDonmus = !_tahtaDonmus;
            KoordinatlariGuncelle();
            TahtayiGuncelle();

            await DisplayAlert("Tahta Döndürüldü",
                $"Tahta {(_tahtaDonmus ? "siyah" : "beyaz")} oyuncu perspektifine döndürüldü.",
                "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Tahta döndürülürken hata: {ex.Message}", "Tamam");
        }
    }




}