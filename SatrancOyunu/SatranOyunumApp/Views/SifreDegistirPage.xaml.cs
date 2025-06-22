using SatranOyunumApp.Services;

namespace SatranOyunumApp.Views;

public partial class SifreDegistirPage : ContentPage
{
    private readonly ISatrancApiService _apiService; 

    
    public SifreDegistirPage(ISatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnSifreDegistirClicked(object sender, EventArgs e)
    {
        try
        {
            // Validasyon kontrolleri (aynı kalacak)
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                MesajGoster("Email adresinizi girin", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(EskiSifreEntry.Text))
            {
                MesajGoster("Mevcut şifrenizi girin", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(YeniSifreEntry.Text))
            {
                MesajGoster("Yeni şifrenizi girin", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(YeniSifreTekrarEntry.Text))
            {
                MesajGoster("Yeni şifrenizi tekrar girin", false);
                return;
            }

            if (YeniSifreEntry.Text != YeniSifreTekrarEntry.Text)
            {
                MesajGoster("Yeni şifreler eşleşmiyor", false);
                return;
            }

            if (YeniSifreEntry.Text.Length < 6)
            {
                MesajGoster("Şifre en az 6 karakter olmalıdır", false);
                return;
            }

            // Loading göster
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MesajLabel.IsVisible = false;

            // API çağrısı
            var sonuc = await _apiService.SifreDegistirAsync(
                EmailEntry.Text.Trim(),
                EskiSifreEntry.Text,
                YeniSifreEntry.Text,
                YeniSifreTekrarEntry.Text
            );

            // Loading gizle
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (sonuc)
            {
                //  Şifre değiştikten sonra güvenlik için çıkış yap

                // 1. Kullanıcı bilgilerini temizle (oturumu kapat)
                Preferences.Remove("KullaniciAdi");
                Preferences.Remove("KullaniciEmail");

                // 2. Başarı mesajı göster
                await DisplayAlert("✅ Başarılı",
                    "Şifreniz başarıyla güncellendi!\n\nGüvenlik nedeniyle oturumunuz kapatılmıştır.\nLütfen yeni şifrenizle tekrar giriş yapın.",
                    "Tamam");

                // 3. Login sayfasına yönlendir
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                MesajGoster("❌ Şifre güncelleme başarısız. Lütfen bilgilerinizi kontrol edin.", false);
            }
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            MesajGoster($"❌ Bir hata oluştu: {ex.Message}", false);
        }
    }

    private async void OnGeriDonTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void MesajGoster(string mesaj, bool basarili)
    {
        MesajLabel.Text = mesaj;
        MesajLabel.TextColor = basarili ? Colors.Green : Colors.Red;
        MesajLabel.IsVisible = true;
    }

    // Sayfa açıldığında focus'u email'e ver
    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();
    }
}