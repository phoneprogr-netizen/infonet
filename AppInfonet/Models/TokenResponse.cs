using System.Text.Json.Serialization;

namespace AppInfonet.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("userName")]
        public string? Username { get; set; }

        [JsonPropertyName("idUser")]
        public string? IdUser { get; set; }

        [JsonPropertyName("primoAccesso")]
        public string? PrimoAccesso { get; set; }

        [JsonPropertyName("cantiere_bool")]
        public string? CantiereBool { get; set; }

        [JsonPropertyName("interventi_bool")]
        public string? InterventiBool { get; set; }

        [JsonPropertyName("mezzi_bool")]
        public string? MezziBool { get; set; }

        [JsonPropertyName(".issued")]
        public string? Issued { get; set; }

        [JsonPropertyName(".expires")]
        public string? Expires { get; set; }
    }
}
