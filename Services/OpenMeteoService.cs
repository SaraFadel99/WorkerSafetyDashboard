using WorkerSafetyDashboard.Models;

namespace WorkerSafetyDashboard.Services
{
    public class OpenMeteoService : IOpenMeteoService
    {
        private readonly HttpClient _httpClient;

        public OpenMeteoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/");
        }

        public async Task<double> GetTemperatureAsync(double latitude, double longitude, DateTimeFilter dateTime)
        {
            var date = dateTime.StartDate; // "2024-07-15"
            var url = $"v1/forecast?latitude={latitude}&longitude={longitude}" +
                      $"&hourly=temperature_2m&start_date={date}&end_date={date}" +
                      $"&temperature_unit=celsius&timezone=UTC";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>();
            if (body?.Hourly?.Time is null || body.Hourly.Time.Count == 0)
                throw new InvalidOperationException("Open-Meteo returned no hourly data");

            var targetTimestamp = $"{date}T{dateTime.StartTime}";
            var index = body.Hourly.Time.FindIndex(t => t == targetTimestamp);

            if (index == -1)
                throw new InvalidOperationException(
                    $"Open-Meteo did not return a matching hour for {targetTimestamp}");

            var temp = body.Hourly.Temperature2m.ElementAtOrDefault(index);
            if (temp is null)
                throw new InvalidOperationException(
                    $"Open-Meteo returned null temperature for {targetTimestamp}");

            return temp.Value;
        }
    }
}