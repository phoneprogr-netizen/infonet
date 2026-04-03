using AppInfonet.Models;
using AppInfonet.Pages;
using AppInfonet.Services;
using Microsoft.Extensions.DependencyInjection;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;


namespace AppInfonet
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();
            builder.UseBarcodeReader();   // 👈 fondamentale per ZXing

            builder.Services.AddSingleton<TokenStore>();
            builder.Services.AddSingleton<SessionStore>();
            builder.Services.AddSingleton<SessionBersagliStore>();
            builder.Services.AddSingleton<SessionDevicesStore>();

            builder.Services.AddHttpClient<AuthApi>(client =>
            {
                client.BaseAddress = new Uri("https://api.infonet.website/");
            });

            builder.Services.AddTransient<Pages.LoginPage>();
            builder.Services.AddTransient<ProfiloPage>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<BersaglioMapEditPage>();

            builder.Services.AddTransient<AppInfonet.Pages.DispositiviPage>();
            builder.Services.AddTransient<AllarmiPage>();
     
            builder.Services.AddTransient<BersagliPage>();
            builder.Services.AddTransient<BersaglioDettaglioPage>();
            builder.Services.AddTransient<BersagliMapPage>();

            return builder.Build();
        }
    }
}
