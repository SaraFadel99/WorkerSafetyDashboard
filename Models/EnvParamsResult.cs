using System.Text.Json.Serialization;

namespace WorkerSafetyDashboard.Models
{
    public class EnvParamsResult
    {
        [JsonPropertyName("metadata")]
        public EnvParamsMetadata Metadata { get; set; } = new();

        [JsonPropertyName("locations")]
        public List<EnvParamsLocation> Locations { get; set; } = new();
    }

    public class EnvParamsMetadata
    {
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("timezone_offset_hours")]
        public double TimezoneOffsetHours { get; set; }

        [JsonPropertyName("time_range")]
        public TimeRange TimeRange { get; set; } = new();

        [JsonPropertyName("timestamps")]
        public List<string> Timestamps { get; set; } = new();
    }

    public class TimeRange
    {
        [JsonPropertyName("start")]
        public string Start { get; set; } = string.Empty;

        [JsonPropertyName("end")]
        public string End { get; set; } = string.Empty;

        [JsonPropertyName("interval")]
        public string Interval { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class EnvParamsLocation
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("elevation")]
        public double Elevation { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("parameters")]
        public EnvParameters Parameters { get; set; } = new();

        [JsonPropertyName("solar_irradiance")]
        public SolarIrradiance? SolarIrradiance { get; set; }
    }

    public class EnvParameters
    {
        [JsonPropertyName("heat_index_celsius")]
        public List<double?> HeatIndexCelsius { get; set; } = new();

        [JsonPropertyName("apparent_temperature_celsius")]
        public List<double?> ApparentTemperatureCelsius { get; set; } = new();

        [JsonPropertyName("relative_humidity_percent")]
        public List<double?> RelativeHumidityPercent { get; set; } = new();

        [JsonPropertyName("wet_bulb_temperature_celsius")]
        public List<double?> WetBulbTemperatureCelsius { get; set; } = new();

        [JsonPropertyName("precipitation_mm")]
        public List<double?> PrecipitationMm { get; set; } = new();

        [JsonPropertyName("cloud_cover_octas")]
        public List<double?> CloudCoverOctas { get; set; } = new();

        // AQI fields — likely unused by your dashboard, but included for completeness
        [JsonPropertyName("air_quality:idx")]
        public List<double?> AirQualityIdx { get; set; } = new();

        [JsonPropertyName("air_quality_pm2p5:idx")]
        public List<double?> AirQualityPm25Idx { get; set; } = new();

        [JsonPropertyName("air_quality_pm10:idx")]
        public List<double?> AirQualityPm10Idx { get; set; } = new();

        [JsonPropertyName("air_quality_no2:idx")]
        public List<double?> AirQualityNo2Idx { get; set; } = new();

        [JsonPropertyName("aqi_us_co")]
        public List<double?> AqiUsCo { get; set; } = new();

        [JsonPropertyName("air_quality_o3:idx")]
        public List<double?> AirQualityO3Idx { get; set; } = new();

        [JsonPropertyName("air_quality_so2:idx")]
        public List<double?> AirQualitySo2Idx { get; set; } = new();

        [JsonPropertyName("methane_ppb")]
        public List<double?> MethanePpb { get; set; } = new();

        [JsonPropertyName("co2_ppm")]
        public List<double?> Co2Ppm { get; set; } = new();
    }

    public class SolarIrradiance
    {
        [JsonPropertyName("clear_sky")]
        public ClearSky ClearSky { get; set; } = new();

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class ClearSky
    {
        [JsonPropertyName("ghi")]
        public double Ghi { get; set; }

        [JsonPropertyName("dni")]
        public double Dni { get; set; }

        [JsonPropertyName("dhi")]
        public double Dhi { get; set; }
    }
}
