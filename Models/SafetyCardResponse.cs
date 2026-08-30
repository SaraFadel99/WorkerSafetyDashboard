namespace WorkerSafetyDashboard.Models
{
    public class SafetyCardResponse
    {
        public string SiteName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timestamp { get; set; } = string.Empty;

        public double HeatIndexF { get; set; }
        public double WetBulbF { get; set; }
        public double HumidityPercent { get; set; }
        public int Aqi { get; set; }
        public double SolarIrradianceGhi { get; set; }
        public bool IsDegraded { get; set; } = false;

        public string Badge { get; set; } = string.Empty;      // e.g. "Danger"
        public string Suggestion { get; set; } = string.Empty; // from Gemini
        public string KeyConcern { get; set; } = string.Empty; // from Gemini
    }
}
