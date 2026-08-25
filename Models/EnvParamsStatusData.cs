using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class EnvParamsStatusData
    {
        [JsonPropertyName("activity_id")]
        public string ActivityId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        public EnvParamsResult? Result { get; set; }
    }
}
