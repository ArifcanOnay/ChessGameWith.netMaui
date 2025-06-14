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
                // Baþarýlý giriþ
                await DisplayAlert("Baþarýlý", $"Hoþgeldin! {EmailEntry.Text}", "Tamam");

                //// Ana sayfaya yönlendir (þimdilik TestPage'e gidelim)
                //await Shell.Current.GoToAsync("//TestPage");
                // Ana sayfaya yönlendir (þimdilik MainPage)
                Application.Current.MainPage = new AppShell();
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
    private async void OnSifreDegistirTapped(object sender, EventArgs e)
    {
        // Þifre deðiþtirme sayfasýna git
        await Shell.Current.GoToAsync("//SifreDegistirPage");
    }

    private async void OnOyunaGitTapped(object sender, EventArgs e)
    {
        // Direkt oyun sayfasýna git (hangi sayfa olduðunu söylerseniz doðru route'u yazarým)
        await Shell.Current.GoToAsync("//MainPage"); // Bu kýsmý oyun sayfanýzýn route'una göre deðiþtirin
    }

    private async void OnKayitOlTapped(object sender, EventArgs e)
    {
        // *** YANLIÞ: "//KullanýcýKaydi" yerine "KullanýcýKaydi" olmalý ***
        await Shell.Current.GoToAsync("KullanýcýKaydi");
    }
}