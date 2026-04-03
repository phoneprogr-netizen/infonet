using AppInfonet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppInfonet.Pages
{
    public partial class ProfiloPage : ContentPage
    {
        private readonly TokenStore _tokenStore;
        private readonly SessionStore _sessionStore;

        public ProfiloPage(TokenStore tokenStore, SessionStore sessionStore)
        {
            InitializeComponent();
            _tokenStore = tokenStore;
            _sessionStore = sessionStore;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Logout",
                "Vuoi davvero uscire?",
                "Si",
                "Annulla");

            if (!confirm)
                return;

            // 🔥 1) Cancello token
            _tokenStore.Clear();

            // 🔥 2) Cancello sessione
            _sessionStore.Clear();

            // 🔥 3) Torno al Login e resetto tutto lo stack
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services != null)
            {
                var loginPage = services.GetRequiredService<LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }
    }
}