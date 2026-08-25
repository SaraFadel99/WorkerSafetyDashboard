using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using WorkerSafetyDashboard.Models;

namespace WorkerSafetyDashboard.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key not configured.");
            _model = config["Gemini:Model"] ?? "gemini-2.5-flash-lite";
        }

        public async Task<SafetySuggestion> GetSafetySuggestionAsync(
            double heatIndexF, double wetBulbF, double humidityPercent,
            int aqi, double solarGhi, SafetyBadge badge)
        {
            string prompt = BuildPrompt(heatIndexF, wetBulbF, humidityPercent, aqi, solarGhi, badge);

            var request = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Parts = new List<GeminiPart> { new GeminiPart { Text = prompt } }
                    }
                }
            };

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(url, request);
                httpResponse.EnsureSuccessStatusCode();

                var geminiResponse = await httpResponse.Content.ReadFromJsonAsync<GeminiResponse>();
                string? rawText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(rawText))
                    return FallbackSuggestion(badge);

                string cleanJson = StripMarkdownFences(rawText);
                var parsed = JsonSerializer.Deserialize<SafetySuggestion>(cleanJson);

                return parsed ?? FallbackSuggestion(badge);
            }
            catch (Exception)
            {
                // Network failure, rate limit (429), malformed JSON, etc.
                // Never let a Gemini failure blank the card.
                return FallbackSuggestion(badge);
            }
        }

        private static string StripMarkdownFences(string text)
        {
            // Gemini sometimes wraps JSON in ```json ... ``` even when told not to
            var match = Regex.Match(text, @"```(?:json)?\s*(\{.*?\})\s*```", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : text.Trim();
        }

        private static SafetySuggestion FallbackSuggestion(SafetyBadge badge) => badge switch
        {
            SafetyBadge.Normal => new SafetySuggestion
            {
                Suggestion = "Conditions are within normal range. Standard hydration practices apply.",
                KeyConcern = "None"
            },
            SafetyBadge.Caution => new SafetySuggestion
            {
                Suggestion = "Encourage regular water breaks and monitor workers for early signs of heat fatigue.",
                KeyConcern = "Elevated heat"
            },
            SafetyBadge.ExtremeCaution => new SafetySuggestion
            {
                Suggestion = "Increase break frequency and consider shifting demanding tasks to cooler hours.",
                KeyConcern = "High heat and humidity"
            },
            SafetyBadge.Danger => new SafetySuggestion
            {
                Suggestion = "Limit outdoor exposure and enforce mandatory shaded rest breaks throughout the shift.",
                KeyConcern = "Dangerous heat levels"
            },
            SafetyBadge.ExtremeDanger => new SafetySuggestion
            {
                Suggestion = "Halt non-essential outdoor work immediately and move workers to a cooled area.",
                KeyConcern = "Critical heat risk"
            },
            _ => new SafetySuggestion { Suggestion = "Unable to generate suggestion.", KeyConcern = "Unknown" }
        };

        private static string BuildPrompt(double heatIndexF, double wetBulbF, double humidityPercent,
            int aqi, double solarGhi, SafetyBadge badge) => $@"
You are a workplace safety assistant for outdoor workers like construction, industrial sites or warehouses in the United States.
Given environmental readings for a specific site and hour, generate a brief, plain-language safety suggestion for a site foreman — not a technician.
Assume the reader has no scientific background.

Environmental data:
- Heat Index: {heatIndexF:F0}°F
- Wet-Bulb Temperature: {wetBulbF:F1}°F
- Relative Humidity: {humidityPercent:F0}%
- Air Quality Index: {aqi}
- Solar Irradiance (GHI): {solarGhi:F0} W/m²
- Risk Level (already determined): {badge}

Instructions:
- Do NOT recalculate or override the given risk level — treat it as fixed.
- Write 1-2 sentences of practical guidance appropriate to this risk level.
- Be specific and actionable (e.g. hydration frequency, shade breaks, rescheduling), not generic.
- Do not use alarming or dramatic language even at high risk levels — stay calm and professional.
- Respond ONLY with valid JSON, no markdown formatting, no code fences, no extra text.

Output format:
{{
  ""suggestion"": ""string, 1-2 sentences"",
  ""key_concern"": ""string, 2-5 words naming the primary hazard driving this risk level""
}}";
    }
}