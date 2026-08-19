using System.Text.Json;
using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class SubmitData
{
    [JsonPropertyName("activity_id")]
    public string ActivityId { get; set; } = string.Empty;
}

public class StatusData
{
    [JsonPropertyName("activity_id")]
    public string ActivityId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "Completed", presumably "Processing"/"Failed" too

    [JsonPropertyName("result")]
    public HeatmapResult? Result { get; set; } // null while still processing
}

public class HeatmapResult
{
    [JsonPropertyName("map_data")]
    public HeatmapFeatureCollection MapData { get; set; } = new();

    [JsonPropertyName("stats_data")]
    public StatsData StatsData { get; set; } = new();
}

public class HeatmapFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("features")]
    public List<HeatmapTileFeature> Features { get; set; } = new();
}

public class HeatmapTileFeature
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public TileProperties Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; } = new();
}

public class TileProperties
{
    [JsonPropertyName("tile_id")]
    public int TileId { get; set; }

    [JsonPropertyName("average_temperature")]
    public double AverageTemperature { get; set; }

    [JsonPropertyName("min_temperature")]
    public double MinTemperature { get; set; }

    [JsonPropertyName("max_temperature")]
    public double MaxTemperature { get; set; }
}

public class StatsData
{
    [JsonPropertyName("temperature_stats")]
    public TemperatureStats TemperatureStats { get; set; } = new();

    [JsonPropertyName("temperature_frequency")]
    public TemperatureFrequency TemperatureFrequency { get; set; } = new();

    // Left as raw JsonElement — only the chart curve data, you likely won't touch this
    [JsonPropertyName("normal_temperature_distribution")]
    public JsonElement NormalTemperatureDistribution { get; set; }
}

public class TemperatureStats
{
    [JsonPropertyName("minimum")]
    public double Minimum { get; set; }

    [JsonPropertyName("maximum")]
    public double Maximum { get; set; }

    [JsonPropertyName("mean")]
    public double Mean { get; set; }

    [JsonPropertyName("standard_deviation")]
    public double StandardDeviation { get; set; }
}

public class TemperatureFrequency
{
    [JsonPropertyName("x_axis")]
    public List<double> XAxis { get; set; } = new();

    [JsonPropertyName("y_axis")]
    public List<int> YAxis { get; set; } = new();
}