using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Http;

namespace WorkerSafetyDashboard.Services
{
    public class FortyGuardService
    {
        private  readonly HttpClient _httpClient;
        public FortyGuardService(IConfiguration configuration)
        {
            var apiKey = configuration["FortyGuard:ApiKey"];
            // client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.fortyguard.com/v1/"),
                DefaultRequestHeaders =
                {
                    { "Accept", "application/json" },
                    { "api-key", $"{apiKey}" } 
                }
            };
        }

        public async Task<string> SubmitHeatmapAsync(HeatmapRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("heatmap", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubmitData>>();
            if (result?.Data is null)
                throw new InvalidOperationException("FortyGuard heatmap submit returned no activity_id");

            return result.Data.ActivityId;
        }

        public async Task<StatusData> GetStatusAsync(string activityId)
        {
            var response = await _httpClient.GetAsync($"status/{activityId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StatusData>>();
            if (result?.Data is null)
                throw new InvalidOperationException("FortyGuard status returned no data");

            return result.Data;
        }


        public async Task<HeatmapResult> PollHeatmapUntilCompleteAsync(
                    string activityId,
                     int maxAttempts = 6)
        {
            var delay = TimeSpan.FromSeconds(3); // starting point: 3s → 6s → 12s → 24s...

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                StatusData status;
                try
                {
                    status = await GetStatusAsync(activityId);
                }
                catch (HttpRequestException ex)
                {
                    // Log activity_id per their "handle failures" guidance — this is your debugging trail
                    throw new InvalidOperationException(
                        $"FortyGuard status check failed for activity {activityId}: {ex.Message}", ex);
                }

                if (status.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && status.Result is not null)
                    return status.Result;

                if (status.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"FortyGuard activity {activityId} failed (no credits deducted)");

                await Task.Delay(delay);
                delay *= 2; // back off: 3s, 6s, 12s, 24s...
            }

            throw new TimeoutException(
                $"FortyGuard activity {activityId} did not complete after {maxAttempts} attempts");
        }


    }
}
