using SatranOyunumApp.Views;

namespace SatranOyunumApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("KullanıcıKaydi", typeof(KullanıcıKaydi));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("SifreDegistirPage", typeof(SifreDegistirPage));
            // Route kayıtları
            Routing.RegisterRoute("GamePage", typeof(GamePage));
        }
    }
}
