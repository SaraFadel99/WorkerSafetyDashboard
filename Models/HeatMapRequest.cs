// Models/FortyGuardModels.cs

using System.Text.Json.Serialization;

public class HeatmapRequest
{
    [JsonPropertyName("polygon_aoi")]
    public FeatureCollection PolygonAoi { get; set; } = new();

    [JsonPropertyName("date_time")]
    public DateTimeFilter DateTime { get; set; } = new();

    [JsonPropertyName("granularity")]
    public int Granularity { get; set; } = 100;
}

public class DateTimeFilter
{
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = string.Empty; // "2024-07-15"

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; } = string.Empty; // "14:00"

    [JsonPropertyName("filter_type")]
    public int FilterType { get; set; } = 1;
}

public class FeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "FeatureCollection";

    [JsonPropertyName("features")]
    public List<GeoFeature> Features { get; set; } = new();
}

public class GeoFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Feature";

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; } = new();
}

public class Geometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Polygon";

    // Polygon coords: [ring][point][lon/lat] — a ring is a closed loop of [lon, lat] pairs
    [JsonPropertyName("coordinates")]
    public List<List<List<double>>> Coordinates { get; set; } = new();
}