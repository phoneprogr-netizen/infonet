using Microsoft.Maui.Storage;

namespace AppInfonet.Services
{
    public class TokenStore
    {
        private const string TokenKey = "auth_token";

        public string? AccessToken { get; private set; }

        public TokenStore()
        {
            AccessToken = Preferences.Default.Get<string?>(TokenKey, null);
        }

        public void SaveToken(string token)
        {
            AccessToken = token;
            Preferences.Default.Set(TokenKey, token);
        }

        public bool HasValidToken()
        {
            // versione semplice: esiste il token = ok
            return !string.IsNullOrEmpty(AccessToken);
        }

        public void Clear()
        {
            AccessToken = null;
            Preferences.Default.Remove(TokenKey);
        }
    }
}