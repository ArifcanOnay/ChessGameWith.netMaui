using Microsoft.Extensions.Logging;
using SatranOyunumApp.Services;
using Microsoft.Extensions.DependencyInjection;
using SatranOyunumApp.Views;

namespace SatranOyunumApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // HttpClient ve Service'i ekle
            builder.Services.AddHttpClient<SatrancApiService>();
         
            builder.Services.AddTransient<TestPage>();
            builder.Services.AddTransient<KullanıcıKaydi>();
            builder.Services.AddTransient<LoginPage>(); // Bu satırı ekleyin
#if DEBUG
            builder.Logging.AddDebug();
            
#endif

            return builder.Build();
        }
    }
}
