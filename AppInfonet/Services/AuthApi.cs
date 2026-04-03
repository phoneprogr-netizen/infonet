using AppInfonet.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TuaApp.Utils;

namespace AppInfonet.Services
{
    public class AuthApi
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStore _tokenStore;
        private readonly SessionStore _sessionStore;

        public AuthApi(HttpClient httpClient, TokenStore tokenStore, SessionStore sessionStore)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
            _sessionStore = sessionStore;
        }

        public async Task<bool> LoginAsync(string mail, string password)
        {
            try
            {
                // costruisco la query
                var url = $"api/Account/LoginuserV2?mail={Uri.EscapeDataString(mail)}&password={Uri.EscapeDataString(password)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginAsync] StatusCode: {(int)response.StatusCode} {response.ReasonPhrase}");
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[LoginAsync] Response JSON: {json}");
                ALog.Info("INFONET", $"[LoginAsync] Response JSON: {json}");

                var tokenResponse = JsonSerializer.Deserialize<LoginUserResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (tokenResponse == null || tokenResponse.User == null)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginAsync] tokenResponse o User null");
                    ALog.Info("INFONET", "LoginAsync] tokenResponse o User null");
                    return false;
                }

                var user = tokenResponse.User;

                if (!user.Abilitato || !user.AccApp)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginAsync] Utente non abilitato all'app");
                    ALog.Info("INFONET", "[LoginAsync] Utente non abilitato all'app");
                    return false;
                }

                _sessionStore.SaveSession(user);
                _sessionStore.SaveLoginCredentials(mail, password);

                return true;
            }
            catch (Exception ex)
            {
                // QUI vedi l’errore vero in Output / LogCat
                System.Diagnostics.Debug.WriteLine($"[LoginAsync] Eccezione: {ex}");
                ALog.Info("INFONET", $"[LoginAsync] Eccezione: {ex}");
                return false;
            }
        }

        public async Task<DashboardV2Response?> GetDashboardDevicesAsync()
        {
            // leggo le credenziali dal SessionStore
            var mail = _sessionStore.Mail;
            var password = _sessionStore.LoginPassword;

            if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password))
            {
                // qui puoi gestire meglio (es. forzare logout)
                throw new InvalidOperationException("Credenziali non presenti in sessione.");
            }

            // costruisco "mail:password" → Base64
            var authString = $"{mail}:{password}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

            // uso una richiesta dedicata, così NON tocco il DefaultRequestHeaders.Authorization (Bearer)
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/History/DashboardV2");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<DashboardV2Response>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result;
        }


        public async Task<List<Bersaglio>> GetBersagliAsync()
        {
            // 1) credenziali in SessionStore (necessarie per Basic)
            var mail = _sessionStore.Mail;
            var password = _sessionStore.LoginPassword;

            if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Credenziali non presenti in sessione.");

            // 2) Basic auth header
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{mail}:{password}"));

            using var request = new HttpRequestMessage(HttpMethod.Get, "api/Target/list");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return new List<Bersaglio>();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var items = await JsonSerializer.DeserializeAsync<List<Bersaglio>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return items ?? new List<Bersaglio>();
        }

        private void SetBasicAuthHeader(string username, string password)
        {
            var authString = $"{username}:{password}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);
        }

    }
}