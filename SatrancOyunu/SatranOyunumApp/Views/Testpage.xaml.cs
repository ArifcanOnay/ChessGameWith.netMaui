using SatranOyunumApp.Services;

namespace SatranOyunumApp.Views;

public partial class TestPage : ContentPage
{
    private readonly ISatrancApiService _apiService;

    public TestPage(ISatrancApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnTestClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Test ediliyor...";

        bool isConnected = await _apiService.TestConnection();

        if (isConnected)
        {
            StatusLabel.Text = "? API Baðlantýsý Baþarýlý!";
        }
        else
        {
            StatusLabel.Text = "? API Baðlantýsý Baþarýsýz!";
        }
    }
}