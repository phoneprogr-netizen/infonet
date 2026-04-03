using System;
using System.Text.Json.Serialization;

namespace AppInfonet.Models
{
    public class AlarmEvent
    {
        [JsonPropertyName("IdTestataAllarme")]
        public int IdTestataAllarme { get; set; }

        [JsonPropertyName("IdImpianto")]
        public int IdImpianto { get; set; }

        [JsonPropertyName("IdStato")]
        public int IdStato { get; set; }

        [JsonPropertyName("DataOra")]
        public DateTime? DataOra { get; set; }

        [JsonPropertyName("DataOraGps")]
        public DateTime? DataOraGps { get; set; }

        [JsonPropertyName("Latitudine")]
        public double? Latitudine { get; set; }

        [JsonPropertyName("Longitudine")]
        public double? Longitudine { get; set; }

        [JsonPropertyName("MessaggioAllarme")]
        public string? MessaggioAllarme { get; set; }

        public string MessaggioDisplay => string.IsNullOrWhiteSpace(MessaggioAllarme) ? "Allarme" : MessaggioAllarme;
        public string DataOraDisplay => DataOra?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        public string StatoDisplay => IdStato.ToString();
    }
}
