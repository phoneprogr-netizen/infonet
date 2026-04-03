using System.Text.Json.Serialization;

namespace AppInfonet.Models
{
    public class LoginUserResponse
    {
        [JsonPropertyName("user")]
        public InfonetUser? User { get; set; }
    }

    public class InfonetUser
    {
        [JsonPropertyName("ID")]
        public int Id { get; set; }

        [JsonPropertyName("GRUPPO")]
        public string Gruppo { get; set; } = string.Empty;

        [JsonPropertyName("USERNAME")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("PASSWORD")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("MAIL")]
        public string Mail { get; set; } = string.Empty;

        [JsonPropertyName("IDCLIENTE")]
        public int IdCliente { get; set; }

        [JsonPropertyName("ACCAPP")]
        public bool AccApp { get; set; }

        [JsonPropertyName("ABILITATO")]
        public bool Abilitato { get; set; }

        // ScadenzaPassword arriva come "/Date(1778364000000)/"
        [JsonPropertyName("ScadenzaPassword")]
        public string? ScadenzaPasswordRaw { get; set; }

        // Se un domani ti servono altri campi, li aggiungi qui.
    }
}