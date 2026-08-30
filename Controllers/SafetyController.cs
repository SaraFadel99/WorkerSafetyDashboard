using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;
using WorkerSafetyDashboard.ExceptionHandling;
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
            double temperatureC=0;
            try
            {
                var dateTimeFilter = new DateTimeFilter
                {
                    StartDate = getDate.ToString("yyyy-MM-dd"),
                    StartTime = getDate.ToString("HH:00"),
                    FilterType = 3 // 3 (Single Day) - requires only start_date//1 for single-hour filtered — required for env_params, per locked architecture
                                   //ToDocould change this
                };


                temperatureC = await _openMeteoService.GetTemperatureAsync(requestData.Lat, requestData.Lon, dateTimeFilter, requestData.TimeZone);

                var envRequest = new EnvParamsRequest
                {
                    Latitude = requestData.Lat,
                    Longitude = requestData.Lon,
                    Temperature = temperatureC,
                    DateTime = dateTimeFilter
                };

                EnvParamsResult envResult = await _fortyGuardService.GetEnvironmentalParametersAsync(envRequest);

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
                    SiteName = requestData.SiteName,
                    Latitude = requestData.Lat,
                    Longitude = requestData.Lon,
                    Timestamp = requestData.NeededDate,
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
            catch (TaskTimeoutException)
            {
                double  humidityPercent = 0;
                int aqi = 0;
                double solarGhi = 0;
            
                // FortyGuard didn't finish in time — degrade gracefully using OpenMeteo temp alone.
                _logger.LogWarning( "env_params timed out, falling back to temperature-only response");

               var isDegraded = true;
                var heatIndexF1 = CToF(temperatureC); // best available proxy — not a real heat index
                double wetBulbF1 = 0;

                var badge1 = HeatSafetyClassifier.ClassifyByHeatIndex(heatIndexF1); // conservative approximation


                var approximatGeminiResult = await _geminiService.GetSafetySuggestionAsync(
                    heatIndexF1, wetBulbF1, humidityPercent, aqi, solarGhi, badge1);

                return Ok(new SafetyCardResponse
                {
                    SiteName = requestData.SiteName,
                    Latitude = requestData.Lat,
                    Longitude = requestData.Lon,
                    Timestamp = requestData.NeededDate,
                    HeatIndexF = Math.Round(heatIndexF1, 1),
                    WetBulbF = Math.Round(wetBulbF1, 1),
                    HumidityPercent = humidityPercent,
                    Aqi = aqi,
                    SolarIrradianceGhi = solarGhi,
                    Badge = badge1.ToString(),
                    Suggestion = approximatGeminiResult.Suggestion,
                    KeyConcern = approximatGeminiResult.KeyConcern,
                    IsDegraded = isDegraded
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