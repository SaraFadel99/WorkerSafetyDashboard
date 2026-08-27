using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Http;
using WorkerSafetyDashboard.ExceptionHandling;
using WorkerSafetyDashboard.Models;

namespace WorkerSafetyDashboard.Services
{
    public class FortyGuardService : IFortyGuardService
    {
        private  readonly HttpClient _httpClient;

        private static readonly HashSet<string> TerminalSuccess = new(StringComparer.OrdinalIgnoreCase) { "succeeded", "completed" };
        private static readonly HashSet<string> TerminalFailure = new(StringComparer.OrdinalIgnoreCase) { "failed", "error" };
        public FortyGuardService(HttpClient httpClient, IConfiguration configuration)
        {
            var apiKey = configuration["FortyGuard:ApiKey"];
            var baseUrl = configuration["FortyGuard:BaseUrl"];

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        //public async Task<string> SubmitHeatmapAsync(HeatmapRequest request)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("heatmap", request);
        //    response.EnsureSuccessStatusCode();

        //    var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubmitData>>();
        //    if (result?.Data is null)
        //        throw new InvalidOperationException("FortyGuard heatmap submit returned no activity_id");

        //    return result.Data.ActivityId;
        //}

        //public async Task<StatusData> GetStatusAsync(string activityId)
        //{
        //    var response = await _httpClient.GetAsync($"status/{activityId}");
        //    response.EnsureSuccessStatusCode();

        //    var result = await response.Content.ReadFromJsonAsync<ApiResponse<StatusData>>();
        //    if (result?.Data is null)
        //        throw new InvalidOperationException("FortyGuard status returned no data");

        //    return result.Data;
        //}


        //public async Task<HeatmapResult> PollHeatmapUntilCompleteAsync(
        //            string activityId,
        //             int maxAttempts = 6)
        //{
        //    var delay = TimeSpan.FromSeconds(3); // starting point: 3s → 6s → 12s → 24s...

        //    for (int attempt = 0; attempt < maxAttempts; attempt++)
        //    {
        //        StatusData status;
        //        try
        //        {
        //            status = await GetStatusAsync(activityId);
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            // Log activity_id per their "handle failures" guidance — this is your debugging trail
        //            throw new InvalidOperationException(
        //                $"FortyGuard status check failed for activity {activityId}: {ex.Message}", ex);
        //        }

        //        if (status.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && status.Result is not null)
        //            return status.Result;

        //        if (status.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        //            throw new InvalidOperationException($"FortyGuard activity {activityId} failed (no credits deducted)");

        //        await Task.Delay(delay);
        //        delay *= 2; // back off: 3s, 6s, 12s, 24s...
        //    }

        //    throw new TimeoutException(
        //        $"FortyGuard activity {activityId} did not complete after {maxAttempts} attempts");
        //}



        //public async Task<string> SubmitEnvParamsAsync(EnvParamsRequest request)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("env_params", request);
        //    response.EnsureSuccessStatusCode();

        //    var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubmitData>>();
        //    if (result?.Data is null)
        //        throw new InvalidOperationException("FortyGuard env_params submit returned no activity_id");

        //    return result.Data.ActivityId;
        //}

        //public async Task<EnvParamsResult> PollEnvParamsUntilCompleteAsync(string activityId, int maxAttempts = 6)
        //{
        //    var delay = TimeSpan.FromSeconds(3);

        //    for (int attempt = 0; attempt < maxAttempts; attempt++)
        //    {
        //        var response = await _httpClient.GetAsync($"status/{activityId}");
        //        response.EnsureSuccessStatusCode();

        //        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<EnvParamsStatusData>>();
        //        var status = wrapper?.Data;

        //        if (status is null)
        //            throw new InvalidOperationException($"FortyGuard status returned no data for {activityId}");

        //        if (status.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && status.Result is not null)
        //            return status.Result;

        //        if (status.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        //            throw new InvalidOperationException($"FortyGuard env_params activity {activityId} failed");

        //        await Task.Delay(delay);
        //        delay *= 2;
        //    }

        //    throw new TimeoutException($"FortyGuard env_params activity {activityId} did not complete after {maxAttempts} attempts");
        //}

        private async Task<string> SubmitAsync(string path, object payload)
    {
        var response = await _httpClient.PostAsJsonAsync(path, payload);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<SubmitData>>();
        if (body?.Error == true || body?.Data is null)
            throw new InvalidOperationException(body?.Message ?? "Submission failed");

        return body.Data.ActivityId;
    }

    // Returns the raw wrapper; caller decides what to do with status/result.
    private async Task<(string Status, T? Result)> GetStatusAsync<T>(string activityId)
    {
        var response = await _httpClient.GetAsync($"v1/status/{activityId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new ActivityNotReadyException(activityId);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<GenericStatusData<T>>>();
        if (body?.Error == true || body?.Data is null)
            throw new InvalidOperationException(body?.Message ?? "Status lookup failed");

        return (body.Data.Status, body.Data.Result);
    }

    private async Task<T> WaitForAsync<T>(
        string activityId,
        double pollIntervalSeconds = 3.0,
        double timeoutSeconds = 60/*sec not sure abu it revisit docs */)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (true)
        {
            (string Status, T? Result) status;
            try
            {
                status = await GetStatusAsync<T>(activityId);
            }
            catch (ActivityNotReadyException)
            {
                // Not queryable yet — keep polling until deadline, same as the reference client.
                if (DateTime.UtcNow >= deadline)
                    throw new TaskTimeoutException($"Activity {activityId} never became visible within {timeoutSeconds}s");

                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
                continue;
            }

            if (TerminalSuccess.Contains(status.Status) && status.Result is not null)
                return status.Result;

            if (TerminalFailure.Contains(status.Status))
                throw new TaskFailedException($"Activity {activityId} failed (status: {status.Status})");

            if (DateTime.UtcNow >= deadline)
                throw new TaskTimeoutException($"Activity {activityId} still '{status.Status}' after {timeoutSeconds}s");

            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
        }
    }

    // ---- Public, single-call-does-both methods ----

    public async Task<EnvParamsResult> GetEnvironmentalParametersAsync(EnvParamsRequest request)
    {
        var activityId = await SubmitAsync("v1/env_params", request);
        return await WaitForAsync<EnvParamsResult>(activityId);
    }

    public async Task<HeatmapResult> CreateHeatmapAsync(HeatmapRequest request)
    {
        var activityId = await SubmitAsync("v1/heatmap", request);
        return await WaitForAsync<HeatmapResult>(activityId);
    }
    }
}
