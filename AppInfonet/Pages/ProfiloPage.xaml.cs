using AppInfonet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppInfonet.Pages
{
    public partial class ProfiloPage : ContentPage
    {
        private readonly TokenStore _tokenStore;
        private readonly SessionStore _sessionStore;
        private readonly AuthApi _authApi;

        public ProfiloPage(TokenStore tokenStore, SessionStore sessionStore, AuthApi authApi)
        {
            InitializeComponent();
            _tokenStore = tokenStore;
            _sessionStore = sessionStore;
            _authApi = authApi;

            MailLabel.Text = string.IsNullOrWhiteSpace(_sessionStore.Mail) ? "mail non disponibile" : _sessionStore.Mail;
            UsernameLabel.Text = string.IsNullOrWhiteSpace(_sessionStore.Username) ? "username non disponibile" : _sessionStore.Username;
            CodiceClienteLabel.Text = $"Codice cliente: {_sessionStore.IdCliente?.ToString() ?? "-"}";
            VersioneLabel.Text = $"Versione {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
        }

        private async void OnModificaProfiloClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services == null)
            {
                await DisplayAlert("Errore", "Servizi applicativi non disponibili.", "OK");
                return;
            }

            var editPage = services.GetRequiredService<ModificaProfiloPage>();
            await Navigation.PushAsync(editPage);
        }

        private async void OnManualeClicked(object sender, EventArgs e)
        {
            await OpenExternalUrlAsync("https://www.infonet.website");
        }

        private async void OnTerminiClicked(object sender, EventArgs e)
        {
            await OpenExternalUrlAsync("https://www.infonet.website/termini_e_condizioni");
        }

        private async Task OpenExternalUrlAsync(string url)
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch
            {
                await DisplayAlert("Errore", "Impossibile aprire il link.", "OK");
            }
        }

        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Eliminazione profilo",
                "Confermi l'invio della richiesta di eliminazione account?",
                "Conferma",
                "Annulla");

            if (!confirm)
                return;

            var result = await _authApi.RequestDeleteProfileAsync();

            await DisplayAlert(
                result.Success ? "Richiesta inviata" : "Errore",
                result.Message,
                "OK");
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

            _tokenStore.Clear();
            _sessionStore.Clear();

            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services != null)
            {
                var loginPage = services.GetRequiredService<LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }
    }
}
