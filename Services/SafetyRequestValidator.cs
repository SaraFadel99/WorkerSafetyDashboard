namespace WorkerSafetyDashboard.Services
{
    public class SafetyRequestValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public static class SafetyRequestValidator
    {
        private static readonly DateTime MinDate = new(2021, 1, 1);
        private static readonly int[] AllowedGranularities = { 60, 80, 100 };

        // Rough US bounding box (continental US; adjust if you need AK/HI)
        private const double UsMinLat = 24.5, UsMaxLat = 49.4;
        private const double UsMinLon = -125.0, UsMaxLon = -66.9;

        public static SafetyRequestValidationResult Validate(
            double lat, double lon, DateTime date, int granularityMeters=100)
        {
            if (lat < UsMinLat || lat > UsMaxLat || lon < UsMinLon || lon > UsMaxLon)
                return Invalid("Coordinates must be within the continental United States.");

            if (date.Date < MinDate || date.Date > DateTime.UtcNow.Date)
                return Invalid($"Date must be between {MinDate:yyyy-MM-dd} and today.");

            if (!AllowedGranularities.Contains(granularityMeters))
                return Invalid("Granularity must be 60, 80, or 100 meters.");

            return new SafetyRequestValidationResult { IsValid = true };
        }

        private static SafetyRequestValidationResult Invalid(string message) =>
            new() { IsValid = false, ErrorMessage = message };
    }
}