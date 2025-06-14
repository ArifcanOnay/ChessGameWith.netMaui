using SatranOyunumApp.Services;

namespace SatranOyunumApp.Views;

public partial class KullanýcýKaydi : ContentPage
{
    private readonly SatrancApiService _apiService;

    public KullanýcýKaydi(SatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnKayitOlClicked(object sender, EventArgs e)
    {
        // Basit validation
        if (string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text))
        {
            await DisplayAlert("Hata", "Kullanýcý adý boþ olamaz!", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Hata", "Email boþ olamaz!", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Þifre boþ olamaz!", "Tamam");
            return;
        }

        // Loading göster
        var loadingButton = sender as Button;
        loadingButton.Text = "Kaydediliyor...";
        loadingButton.IsEnabled = false;

        try
        {
            // API'ye kayýt isteði gönder
            var sonuc = await _apiService.KullaniciKaydet(
                KullaniciAdiEntry.Text,
                EmailEntry.Text,
                SifreEntry.Text
            );

            if (sonuc.Basarili)
            {
                await DisplayAlert("Baþarýlý", sonuc.Mesaj, "Tamam");

                // Form'u temizle
                KullaniciAdiEntry.Text = "";
                EmailEntry.Text = "";
                SifreEntry.Text = "";

                // *** DEÐÝÞTÝRÝLEN: Route ismiyle geri dön ***
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await DisplayAlert("Hata", sonuc.Mesaj, "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Beklenmeyen hata: {ex.Message}", "Tamam");
        }
        finally
        {
            // Button'u eski haline getir
            loadingButton.Text = "KAYIT OL";
            loadingButton.IsEnabled = true;
        }
    }
}