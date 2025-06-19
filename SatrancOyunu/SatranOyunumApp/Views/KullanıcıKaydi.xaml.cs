using SatranOyunumApp.Services;

namespace SatranOyunumApp.Views;

public partial class KullanıcıKaydi : ContentPage
{
    private readonly ISatrancApiService _apiService; 

    // Constructor'da Interface kullan
    public KullanıcıKaydi(ISatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnKayitOlClicked(object sender, EventArgs e)
    {
        // Basit validation
        if (string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text))
        {
            await DisplayAlert("Hata", "Kullanıcı adı boş olamaz!", "Tamam");
            return;
        }
        // Email doğrulaması 
        if (!EmailDogrula(EmailEntry.Text))
        {
            await DisplayAlert("Geçersiz Email",
                "Lütfen geçerli bir Gmail adresi giriniz.\nÖrnek: kullaniciadi@gmail.com",
                "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Hata", "Email boş olamaz!", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Şifre boş olamaz!", "Tamam");
            return;
        }

        // Loading göster
        var loadingButton = sender as Button;
        loadingButton.Text = "Kaydediliyor...";
        loadingButton.IsEnabled = false;

        try
        {
            // API'ye kayıt isteği gönder
            var sonuc = await _apiService.KullaniciKaydet(
                KullaniciAdiEntry.Text,
                EmailEntry.Text,
                SifreEntry.Text
            );

            if (sonuc.Basarili)
            {
                await DisplayAlert("Başarılı", sonuc.Mesaj, "Tamam");

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
    /// Email adresinin @gmail.com ile bitip bitmediğini kontrol eder
    /// </summary>
    /// <param name="email">Kontrol edilecek email adresi</param>
    /// <returns>Geçerli ise true, değilse false</returns>
    private bool EmailDogrula(string email)
    {
        // Null veya boş kontrol
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Trim ile başındaki ve sonundaki boşlukları temizle
        email = email.Trim().ToLower();

        // @ işareti var mı kontrol et
        if (!email.Contains("@"))
            return false;

        // @gmail.com ile bitiyor mu kontrol et
        if (!email.EndsWith("@gmail.com"))
            return false;

        // @ işaretinden önce en az 1 karakter var mı kontrol et
        string kullaniciAdi = email.Substring(0, email.IndexOf("@"));
        if (string.IsNullOrWhiteSpace(kullaniciAdi))
            return false;

        // Kullanıcı adında geçersiz karakterler var mı kontrol et
        if (kullaniciAdi.Contains(" ") || kullaniciAdi.Contains(".."))
            return false;

        return true;
    }
}