using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace berber_randevu_uygulamasi
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // -----------------------------
            //  CONFIG
            // -----------------------------
            builder.Services.AddSingleton(ApiConfig.Current);

            // -----------------------------
            //  HTTP + ApiClient
            // -----------------------------
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<ApiConfig>();

                // Tek HttpClient instance (App ömrü boyunca)
                var http = new HttpClient
                {
                    BaseAddress = new Uri(cfg.BaseUrl),
                    Timeout = TimeSpan.FromSeconds(cfg.TimeoutSeconds)
                };

                // İstersen default header vb. burada
                // http.DefaultRequestHeaders.Add("Accept", "application/json");

                return http;
            });

            builder.Services.AddSingleton<ApiClient>();

            // -----------------------------
            //  PAGES
            //  (Her sayfa constructor(ApiClient api) alacak)
            // -----------------------------
            builder.Services.AddTransient<GirisSayfasi>();
            // Diğer sayfaları da bu şekilde ekleyeceksin:
            // builder.Services.AddTransient<BerberAnaPanelSayfasi>();
            // builder.Services.AddTransient<RandevuOlusturSayfasi>();
            // ...
            builder.Services.AddSingleton<NavigationServices>();
            return builder.Build();
        }
    }
}