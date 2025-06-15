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
        // *** YENÝ: Email doðrulamasý ***
        if (!EmailDogrula(EmailEntry.Text))
        {
            await DisplayAlert("Geçersiz Email",
                "Lütfen geçerli bir Gmail adresi giriniz.\nÖrnek: kullaniciadi@gmail.com",
                "Tamam");
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
    /// <summary>
    /// Email adresinin @gmail.com ile bitip bitmediðini kontrol eder
    /// </summary>
    /// <param name="email">Kontrol edilecek email adresi</param>
    /// <returns>Geçerli ise true, deðilse false</returns>
    private bool EmailDogrula(string email)
    {
        // Null veya boþ kontrol
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Trim ile baþýndaki ve sonundaki boþluklarý temizle
        email = email.Trim().ToLower();

        // @ iþareti var mý kontrol et
        if (!email.Contains("@"))
            return false;

        // @gmail.com ile bitiyor mu kontrol et
        if (!email.EndsWith("@gmail.com"))
            return false;

        // @ iþaretinden önce en az 1 karakter var mý kontrol et
        string kullaniciAdi = email.Substring(0, email.IndexOf("@"));
        if (string.IsNullOrWhiteSpace(kullaniciAdi))
            return false;

        // Kullanýcý adýnda geçersiz karakterler var mý kontrol et
        if (kullaniciAdi.Contains(" ") || kullaniciAdi.Contains(".."))
            return false;

        return true;
    }
}