using System;
using WorkerSafetyDashboard.Services;

// TODO: TEMPORARY - remove once live env_params is wired into SafetyController (Day C target)
// Used only for Gemini prompt testing and controller scaffolding with mock/fixed data.
namespace WorkerSafetyDashboard.MockData
{
    public class EnvParamsMockFixture
    {
        public string Label { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double HeatIndexC { get; set; }
        public double WetBulbC { get; set; }
        public double HumidityPercent { get; set; }
        public int Aqi { get; set; }
        public double SolarIrradianceWm2 { get; set; }
        public SafetyBadge ExpectedBadge { get; set; } // sanity anchor, not sent to Gemini
    }

    public static class EnvParamsMockData
    {
        public static readonly List<EnvParamsMockFixture> Fixtures = new()
    {
        new EnvParamsMockFixture
        {
            Label = "Normal - mild day",
            Latitude = 33.4484, Longitude = -112.0740, // Phoenix, AZ
            HeatIndexC = 23.9,   // ~75°F
            WetBulbC = 18.0,
            HumidityPercent = 35,
            Aqi = 42,
            SolarIrradianceWm2 = 450,
            ExpectedBadge = SafetyBadge.Normal
        },
        new EnvParamsMockFixture
        {
            Label = "Caution - warm afternoon",
            Latitude = 29.7604, Longitude = -95.3698, // Houston, TX
            HeatIndexC = 29.4,   // ~85°F
            WetBulbC = 24.5,//~76°F.
            HumidityPercent = 55,
            Aqi = 68,
            SolarIrradianceWm2 = 700,
            ExpectedBadge = SafetyBadge.Caution
        },
        new EnvParamsMockFixture
        {
            Label = "Extreme Caution - hot humid",
            Latitude = 25.7617, Longitude = -80.1918, // Miami, FL
            HeatIndexC = 35.0,   // ~95°F
            WetBulbC = 27.8, 
            HumidityPercent = 70,
            Aqi = 85,
            SolarIrradianceWm2 = 850,
            ExpectedBadge = SafetyBadge.ExtremeCaution
        },
        new EnvParamsMockFixture
        {
            Label = "Danger - peak summer",
            Latitude = 33.4484, Longitude = -112.0740, // Phoenix, AZ
            HeatIndexC = 43.3,   // ~110°F
            WetBulbC = 29.5,
            HumidityPercent = 40,
            Aqi = 120,
            SolarIrradianceWm2 = 950,
            ExpectedBadge = SafetyBadge.Danger
        },
        new EnvParamsMockFixture
        {
            Label = "Extreme Danger - severe heatwave",
            Latitude = 36.1699, Longitude = -115.1398, // Las Vegas, NV
            HeatIndexC = 54.4,   // ~130°F
            WetBulbC = 31.0,
            HumidityPercent = 25,
            Aqi = 155,
            SolarIrradianceWm2 = 1000,
            ExpectedBadge = SafetyBadge.ExtremeDanger
        }
    };
    }
}
