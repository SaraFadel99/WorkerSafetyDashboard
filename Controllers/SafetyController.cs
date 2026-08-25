using Microsoft.AspNetCore.Mvc;
using WorkerSafetyDashboard.Models;
using WorkerSafetyDashboard.Services;
using WorkerSafetyDashboard.MockData;

namespace WorkerSafetyDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SafetyController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public SafetyController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpGet]
        public async Task<ActionResult<SafetyCardResponse>> GetSafetyCard(
            [FromQuery] double lat,
            [FromQuery] double lon,
            [FromQuery] DateTime date,
            [FromQuery] int granularity = 100)
        {
            var validation = SafetyRequestValidator.Validate(lat, lon, date, granularity);
            if (!validation.IsValid)
                return BadRequest(new { error = validation.ErrorMessage });

            // TODO: TEMPORARY - swap for live GetLiveEnvParams(lat, lon, date) once Day-of-live-wiring lands
            var fixture = GetMockFixtureNearest(lat, lon);

            double heatIndexF = CToF(fixture.HeatIndexC);
            double wetBulbF = CToF(fixture.WetBulbC);
            var badge = HeatSafetyClassifier.ClassifyByHeatIndex(heatIndexF);

            var geminiResult = await _geminiService.GetSafetySuggestionAsync(
                heatIndexF, wetBulbF, fixture.HumidityPercent,
                fixture.Aqi, fixture.SolarIrradianceWm2, badge);

            return Ok(new SafetyCardResponse
            {
                Latitude = lat,
                Longitude = lon,
                Timestamp = date.ToString("o"),
                HeatIndexF = Math.Round(heatIndexF, 1),
                WetBulbF = Math.Round(wetBulbF, 1),
                HumidityPercent = fixture.HumidityPercent,
                Aqi = fixture.Aqi,
                SolarIrradianceGhi = fixture.SolarIrradianceWm2,
                Badge = badge.ToString(),
                Suggestion = geminiResult.Suggestion,
                KeyConcern = geminiResult.KeyConcern
            });
        }

        // Temporary: just returns a mock fixture regardless of exact coords, so any click works during FE dev.
        private static EnvParamsMockFixture GetMockFixtureNearest(double lat, double lon)
        {
            return EnvParamsMockData.Fixtures[0]; // swap logic later; fine for wiring FE now
        }

        private static double CToF(double celsius) => (celsius * 9.0 / 5.0) + 32;
    }
}