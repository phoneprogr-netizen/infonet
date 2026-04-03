using AppInfonet.Pages;
using AppInfonet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppInfonet
{
    public partial class App : Application
    {
        public App(IServiceProvider services, TokenStore tokenStore, SessionStore sessionStore)
        {
            InitializeComponent();

            // 1) Ricarico i dati salvati
            sessionStore.RestoreFromPreferences();

            // 2) Scelgo la pagina iniziale
            Page startPage;

            if (sessionStore.CanAutoLogin())
            {
                // Autologin valido → Dashboard
                startPage = services.GetRequiredService<DashboardPage>();
            }
            else
            {
                // No autologin → Login
                startPage = services.GetRequiredService<LoginPage>();
            }

            // 3) ✅ Sempre NavigationPage (così PushAsync funziona ovunque)
            MainPage = new NavigationPage(startPage);
        }

    }

}
