using WorkerSafetyDashboard.Models;

namespace WorkerSafetyDashboard.Services
{
    public interface IOpenMeteoService
    {
        Task<double> GetTemperatureAsync(double latitude, double longitude, DateTimeFilter dateTime);
    }
}