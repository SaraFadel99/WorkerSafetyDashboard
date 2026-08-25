using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    // What you actually parse the model's text output INTO
    public class SafetySuggestion
    {
        [JsonPropertyName("suggestion")]
        public string Suggestion { get; set; } = string.Empty;

        [JsonPropertyName("key_concern")]
        public string KeyConcern { get; set; } = string.Empty;
    }
}
