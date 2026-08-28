using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkerSafetyDashboard.Models;
using WorkerSafetyDashboard.Services;

namespace WorkerSafetyDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SafetyController : ControllerBase
    {
        private readonly IFortyGuardService _fortyGuardService;
        private readonly IOpenMeteoService _openMeteoService;
        private readonly GeminiService _geminiService;
        private readonly ILogger<SafetyController> _logger;
        public SafetyController(IFortyGuardService fortyGuardService,
                                IOpenMeteoService openMeteoService,
                                GeminiService geminiService,
                                ILogger<SafetyController> logger)

        {
            _fortyGuardService = fortyGuardService;
            _openMeteoService = openMeteoService;
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("locationSafety")]
        public async Task<ActionResult<SafetyCardResponse>> GetSafetyCard( SafetyCardRequest requestData)
        {
            DateTime getDate = DateTime.Parse(requestData.NeededDate);

            if (requestData is null)
                return BadRequest(new { error = "Request body is required." });
            var validation = SafetyRequestValidator.Validate(requestData.Lat, requestData.Lon, getDate, requestData.Granularity);
            if (!validation.IsValid)
                return BadRequest(new { error = validation.ErrorMessage });
            if (!DateTime.TryParse(requestData.NeededDate, out var neededDate))
                return BadRequest(new { error = "NeededDate is not a valid date/time string." });

            try
            {
                var dateTimeFilter = new DateTimeFilter
                {
                    StartDate = getDate.ToString("yyyy-MM-dd"),
                    StartTime = getDate.ToString("HH:00"),
                    FilterType = 1 // single-hour filtered — required for env_params, per locked architecture
                                       //ToDocould change this
                };

             
                var temperatureC = await _openMeteoService.GetTemperatureAsync(requestData.Lat, requestData.Lon, dateTimeFilter, requestData.TimeZone);

                var envRequest = new EnvParamsRequest
                {
                    Latitude = requestData.Lat,
                    Longitude = requestData.Lon,
                    Temperature = temperatureC,
                    DateTime = dateTimeFilter
                };

                var envResult = await _fortyGuardService.GetEnvironmentalParametersAsync(envRequest);

                var location = envResult.Locations.FirstOrDefault();
                if (location is null)
                    throw new InvalidOperationException("env_params returned no locations");

                double? heatIndexC = location.Parameters.HeatIndexCelsius.FirstOrDefault();
                double? wetBulbC = location.Parameters.WetBulbTemperatureCelsius.FirstOrDefault();
                double? humidityPercent = location.Parameters.RelativeHumidityPercent.FirstOrDefault();
                double? aqiRaw = location.Parameters.AirQualityIdx.FirstOrDefault();
                double solarGhi = location.SolarIrradiance?.ClearSky.Ghi ?? 0;

                if (heatIndexC is null || wetBulbC is null || humidityPercent is null)
                    throw new InvalidOperationException("env_params returned null for a required field");

                double heatIndexF = CToF(heatIndexC.Value);
                double wetBulbF = CToF(wetBulbC.Value);
                int aqi = (int)Math.Round(aqiRaw ?? 0);

                var badge = HeatSafetyClassifier.ClassifyByHeatIndex(heatIndexF);

                var geminiResult = await _geminiService.GetSafetySuggestionAsync(
                    heatIndexF, wetBulbF, humidityPercent.Value, aqi, solarGhi, badge);

                return Ok(new SafetyCardResponse
                {
                    SiteName=requestData.SiteName,
                    Latitude = requestData.Lat,
                    Longitude = requestData.Lon,
                    Timestamp = requestData.NeededDate,//.ToString("o"),
                    HeatIndexF = Math.Round(heatIndexF, 1),
                    WetBulbF = Math.Round(wetBulbF, 1),
                    HumidityPercent = humidityPercent.Value,
                    Aqi = aqi,
                    SolarIrradianceGhi = solarGhi,
                    Badge = badge.ToString(),
                    Suggestion = geminiResult.Suggestion,
                    KeyConcern = geminiResult.KeyConcern
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        private static double CToF(double celsius) => (celsius * 9.0 / 5.0) + 32;
    }
}