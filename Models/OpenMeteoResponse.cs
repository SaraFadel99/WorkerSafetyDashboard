using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class OpenMeteoResponse
    {
        [JsonPropertyName("hourly")]
        public OpenMeteoHourly Hourly { get; set; } = new();
    }

    public class OpenMeteoHourly
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m")]
        public List<double?> Temperature2m { get; set; } = new();
    }
}