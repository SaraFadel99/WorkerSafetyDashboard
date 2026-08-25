using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class GenericStatusData<T>
    {
        [JsonPropertyName("activity_id")]
        public string ActivityId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        public T? Result { get; set; }
    }
}
