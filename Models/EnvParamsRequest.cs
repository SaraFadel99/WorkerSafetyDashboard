using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class EnvParamsRequest
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } // sourced from a heatmap tile at this same point/date/time

        [JsonPropertyName("date_time")]
        public DateTimeFilter DateTime { get; set; } = new(); // reuse existing class
    }
}



