namespace WorkerSafetyDashboard.Models
{
    public class SafetyCardRequest
    {
        public string SiteName { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string NeededDate { get; set; }
        public string TimeZone { get; set; }
        public int Granularity { get; set; } = 100;

    }
}
