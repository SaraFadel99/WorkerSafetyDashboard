using WorkerSafetyDashboard.Models;

namespace WorkerSafetyDashboard.Services
{
    public interface IFortyGuardService
    {
         Task<EnvParamsResult> GetEnvironmentalParametersAsync(EnvParamsRequest request);
        //  Task<StatusData> GetStatusAsync(string activityId);
        //    Task<HeatmapResult> PollHeatmapUntilCompleteAsync(string activityId, int maxAttempts = 6);
    }
}
