namespace WorkerSafetyDashboard.Services
{
    public enum SafetyBadge
    {
        Normal,
        Caution,
        ExtremeCaution,
        Danger,
        ExtremeDanger
    }

    public static class HeatSafetyClassifier
    {
        /// <summary>
        /// Classifies risk based on NWS Heat Index thresholds (°F).
        /// Source: NWS Heat Index chart / OSHA-NIOSH occupational heat guidance.
        /// </summary>
        public static SafetyBadge ClassifyByHeatIndex(double heatIndexF)
        {
            if (heatIndexF < 80) return SafetyBadge.Normal;
            if (heatIndexF < 90) return SafetyBadge.Caution;
            if (heatIndexF < 103) return SafetyBadge.ExtremeCaution;
            if (heatIndexF < 125) return SafetyBadge.Danger;
            return SafetyBadge.ExtremeDanger;
        }
    }

    public static class TemperatureConverter
    {
        public static double CelsiusToFahrenheit(double celsius) => (celsius * 9.0 / 5.0) + 32;
    }

}
