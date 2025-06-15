using SatranOyunumApp.Services;

namespace SatranOyunumApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly SatrancApiService _apiService;

    public LoginPage(SatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnGirisYapClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Hata", "Email alaný boþ olamaz", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Þifre alaný boþ olamaz", "Tamam");
            return;
        }

        // Loading göster
        GirisButton.Text = "Giriþ yapýlýyor...";
        GirisButton.IsEnabled = false;

        try
        {
            // API'den login kontrolü yap
            var loginSonucu = await _apiService.Login(EmailEntry.Text, SifreEntry.Text);

            if (loginSonucu.Basarili)
            {
                if (loginSonucu.Basarili)
                {
                    // *** YENÝ: Baþarýlý giriþten sonra hoþgeldin ekranýný göster ***
                    HosgeldinEkraniniGoster(loginSonucu);
                }

                //Application.Current.MainPage = new AppShell();
            }
            else
            {
                // Hatalý giriþ
                await DisplayAlert("Hata", loginSonucu.Mesaj, "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Bir hata oluþtu: " + ex.Message, "Tamam");
        }
        finally
        {
            // Loading'i kapat
            GirisButton.Text = "GÝRÝÞ YAP";
            GirisButton.IsEnabled = true;
        }
    }

    private void HosgeldinEkraniniGoster(LoginSonucu loginSonucu)
    {
        // Login formunu gizle
        LoginFormFrame.IsVisible = false;

        // Hoþgeldin ekranýný göster
        HosgeldinFrame.IsVisible = true;

        // Kullanýcý adýný göster
        HosgeldinLabel.Text = $"Hoþgeldiniz {loginSonucu.KullaniciAdi}!";
        Preferences.Set("KullaniciAdi", loginSonucu.KullaniciAdi ?? "Kullanýcý");
        Preferences.Set("KullaniciEmail", EmailEntry.Text);
    }

    private void OnCikisYapClicked(object sender, EventArgs e)
    {
        // Hoþgeldin ekranýný gizle
        HosgeldinFrame.IsVisible = false;

        // Login formunu göster
        LoginFormFrame.IsVisible = true;

        // Form alanlarýný temizle
        EmailEntry.Text = "";
        SifreEntry.Text = "";
    }

    private async void OnSifreDegistirTapped(object sender, EventArgs e)
    {
        // Þifre deðiþtirme sayfasýna git
        await Shell.Current.GoToAsync("//SifreDegistirPage");
    }

    private async void OnOyunaGitTapped(object sender, EventArgs e)
    {
        try
        {
            // GamePage'e git
            await Shell.Current.GoToAsync("//GamePage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Sayfa yönlendirme hatasý: {ex.Message}", "Tamam");
        }
    }

    private async void OnKayitOlTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("KullanýcýKaydi");
    }
}