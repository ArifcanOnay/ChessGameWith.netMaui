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



            // ✅ Service'leri DI Container'a kaydet
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<ISatrancApiService, SatrancApiService>();

            // ✅ Page'leri kaydet
            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<KullanıcıKaydi>();
            builder.Services.AddTransient<SifreDegistirPage>();
            builder.Services.AddTransient<TestPage>();
#if DEBUG
            builder.Logging.AddDebug();
            
#endif

            return builder.Build();
        }
    }
}
