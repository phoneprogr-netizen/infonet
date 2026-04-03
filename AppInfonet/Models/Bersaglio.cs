using System.Text.Json.Serialization;

namespace AppInfonet.Models
{
    public class Bersaglio
    {
        [JsonPropertyName("ID")]
        public int Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("polyline_javascript")]
        public string? PolylineJavascript { get; set; }

        [JsonPropertyName("polyline_array")]
        public List<BersaglioPoint> PolylineArray { get; set; } = new();

        [JsonPropertyName("devices")]
        public List<BersaglioDevice> Devices { get; set; } = new();
    }

    public class BersaglioPoint
    {
        [JsonPropertyName("Latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("Longitude")]
        public double Longitude { get; set; }
    }

    public class BersaglioDevice
    {
        [JsonPropertyName("codperif")]
        public string CodPerif { get; set; } = string.Empty;

        [JsonPropertyName("Plate")]
        public string Plate { get; set; } = string.Empty;

        [JsonPropertyName("AlarmOnEnter")]
        public bool AlarmOnEnter { get; set; }

        [JsonPropertyName("AlarmOnExit")]
        public bool AlarmOnExit { get; set; }

        [JsonPropertyName("AlarmOnStart")]
        public bool AlarmOnStart { get; set; }

        [JsonPropertyName("AlarmOnStop")]
        public bool AlarmOnStop { get; set; }
    }
}