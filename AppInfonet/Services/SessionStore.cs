using AppInfonet.Models;
using Microsoft.Maui.Storage;

namespace AppInfonet.Services
{
    public class SessionStore
    {
        private const string KeyUserId = "Session_UserId";
        private const string KeyUsername = "Session_Username";
        private const string KeyGruppo = "Session_Gruppo";
        private const string KeyMail = "Session_Mail";
        private const string KeyIdCliente = "Session_IdCliente";
        private const string KeyAccApp = "Session_AccApp";
        private const string KeyAbilitato = "Session_Abilitato";
        private const string KeyPasswordExp = "Session_PasswordExp";

        private const string KeyLoginMail = "Session_LoginMail";
        private const string KeyLoginPassword = "Session_LoginPassword";
        private const string KeyAutoLoginUntil = "Session_AutoLoginUntil";

        // Proprietà in memoria
        public int? UserId { get; private set; }
        public string? Username { get; private set; }
        public string? Gruppo { get; private set; }
        public string? Mail { get; private set; }
        public int? IdCliente { get; private set; }
        public bool AccApp { get; private set; }
        public bool Abilitato { get; private set; }
        public DateTimeOffset? PasswordExpiration { get; private set; }

        public string? LoginMail { get; private set; }
        public string? LoginPassword { get; private set; }
        public DateTimeOffset? AutoLoginUntil { get; private set; }

        // 🔹 1) Salva i dati utente restituiti dall'API
        public void SaveSession(InfonetUser user)
        {
            UserId = user.Id;
            Username = user.Username;
            Gruppo = user.Gruppo;
            Mail = user.Mail;
            IdCliente = user.IdCliente;
            AccApp = user.AccApp;
            Abilitato = user.Abilitato;
            PasswordExpiration = ParseMicrosoftDate(user.ScadenzaPasswordRaw);

            Preferences.Set(KeyUserId, UserId ?? 0);
            Preferences.Set(KeyUsername, Username ?? string.Empty);
            Preferences.Set(KeyGruppo, Gruppo ?? string.Empty);
            Preferences.Set(KeyMail, Mail ?? string.Empty);
            Preferences.Set(KeyIdCliente, IdCliente ?? 0);
            Preferences.Set(KeyAccApp, AccApp);
            Preferences.Set(KeyAbilitato, Abilitato);
            Preferences.Set(KeyPasswordExp, PasswordExpiration?.ToUnixTimeSeconds() ?? 0);

            System.Diagnostics.Debug.WriteLine(
                $"[SessionStore.SaveSession] UserId={UserId}, Username={Username}, Gruppo={Gruppo}, Mail={Mail}, IdCliente={IdCliente}, AccApp={AccApp}, Abilitato={Abilitato}, PasswordExp={PasswordExpiration}"
            );
        }

        // 🔹 2) Salva credenziali usate per il login + autologin 30 giorni
        public void SaveLoginCredentials(string mailOrUsername, string password)
        {
            LoginMail = mailOrUsername;
            LoginPassword = password;
            AutoLoginUntil = DateTimeOffset.Now.AddDays(30);   // ultimo login + 30gg

            Preferences.Set(KeyLoginMail, LoginMail ?? string.Empty);
            Preferences.Set(KeyLoginPassword, LoginPassword ?? string.Empty);
            Preferences.Set(KeyAutoLoginUntil, AutoLoginUntil.Value.ToUnixTimeSeconds());

            System.Diagnostics.Debug.WriteLine(
                $"[SessionStore.SaveLoginCredentials] LoginMail={LoginMail}, AutoLoginUntil={AutoLoginUntil}"
            );
        }

        // 🔹 3) Richiama al bootstrap per ricaricare tutto da Preferences
        public void RestoreFromPreferences()
        {
            var userId = Preferences.Get(KeyUserId, 0);
            UserId = userId == 0 ? null : userId;

            Username = Preferences.Get(KeyUsername, null);
            Gruppo = Preferences.Get(KeyGruppo, null);
            Mail = Preferences.Get(KeyMail, null);

            var idCliente = Preferences.Get(KeyIdCliente, 0);
            IdCliente = idCliente == 0 ? null : idCliente;

            AccApp = Preferences.Get(KeyAccApp, false);
            Abilitato = Preferences.Get(KeyAbilitato, false);

            var pwdExpSec = Preferences.Get(KeyPasswordExp, 0L);
            PasswordExpiration = pwdExpSec > 0 ? DateTimeOffset.FromUnixTimeSeconds(pwdExpSec) : null;

            LoginMail = Preferences.Get(KeyLoginMail, null);
            LoginPassword = Preferences.Get(KeyLoginPassword, null);

            var autoLoginSec = Preferences.Get(KeyAutoLoginUntil, 0L);
            AutoLoginUntil = autoLoginSec > 0 ? DateTimeOffset.FromUnixTimeSeconds(autoLoginSec) : null;

            System.Diagnostics.Debug.WriteLine(
                $"[SessionStore.RestoreFromPreferences] UserId={UserId}, Username={Username}, Gruppo={Gruppo}, Mail={Mail}, IdCliente={IdCliente}, AccApp={AccApp}, Abilitato={Abilitato}, LoginMail={LoginMail}, AutoLoginUntil={AutoLoginUntil}"
            );
        }

        public bool IsSessionComplete()
        {
            // campi minimi per considerare la sessione "affidabile"
            return UserId.HasValue
                && UserId.Value > 0
                && !string.IsNullOrWhiteSpace(Mail)
                && !string.IsNullOrWhiteSpace(Username)
                && IdCliente.HasValue
                && IdCliente.Value > 0;
        }

        public bool CanAutoLogin()
        {
            // se non ho caricato ancora, provo a ripristinare
            if (AutoLoginUntil == null && (LoginMail == null || LoginPassword == null))
                RestoreFromPreferences();

            return !string.IsNullOrEmpty(LoginMail)
                   && !string.IsNullOrEmpty(LoginPassword)
                   && AutoLoginUntil.HasValue
                   && AutoLoginUntil.Value > DateTimeOffset.Now
                   && Abilitato
                   && AccApp;
        }

        //public bool CanAutoLogin()
        //{
        //    return IsSessionComplete()
        //        && !string.IsNullOrEmpty(LoginMail)
        //        && !string.IsNullOrEmpty(LoginPassword)
        //        && AutoLoginUntil.HasValue
        //        && AutoLoginUntil.Value > DateTimeOffset.Now
        //        && Abilitato
        //        && AccApp;
        //}

        // 🔹 5) Clear completo (logout)
        public void Clear()
        {
            UserId = null;
            Username = null;
            Gruppo = null;
            Mail = null;
            IdCliente = null;
            AccApp = false;
            Abilitato = false;
            PasswordExpiration = null;

            LoginMail = null;
            LoginPassword = null;
            AutoLoginUntil = null;

            Preferences.Remove(KeyUserId);
            Preferences.Remove(KeyUsername);
            Preferences.Remove(KeyGruppo);
            Preferences.Remove(KeyMail);
            Preferences.Remove(KeyIdCliente);
            Preferences.Remove(KeyAccApp);
            Preferences.Remove(KeyAbilitato);
            Preferences.Remove(KeyPasswordExp);

            Preferences.Remove(KeyLoginMail);
            Preferences.Remove(KeyLoginPassword);
            Preferences.Remove(KeyAutoLoginUntil);
        }

        // 🔹 helper per "/Date(1778364000000)/"
        private static DateTimeOffset? ParseMicrosoftDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // formato tipo "/Date(1778364000000)/"
            var start = value.IndexOf('(');
            var end = value.IndexOf(')');
            if (start < 0 || end <= start + 1)
                return null;

            var numberStr = value.Substring(start + 1, end - start - 1);
            if (!long.TryParse(numberStr, out var ms))
                return null;

            // millisecondi dall'epoch
            return DateTimeOffset.FromUnixTimeMilliseconds(ms);
        }
    }
}