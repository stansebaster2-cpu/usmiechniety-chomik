using ChomikApp.Localization;

namespace ChomikEngine.Design;

public enum HamsterSpecies
{
    Syrian,
    Dwarf,
    Roborovski,
    ChineseStriped
}

public enum SpineRiskLevel { None, Low, Medium, High }

public record HealthQuery(
    double WheelDiameterMm,
    HamsterSpecies Species,
    double TargetSpeedKmh = 2.0,
    double DailyDistanceKm = 6.5
);

public record HealthResult(
    double Rpm,
    double DurationHours,
    SpineRiskLevel SpineRisk,
    string Recommendation
);

public static class HamsterHealthCalc
{
    private static readonly Dictionary<HamsterSpecies, double> MinDiameter = new()
    {
        [HamsterSpecies.Syrian] = 250,
        [HamsterSpecies.Dwarf] = 200,
        [HamsterSpecies.Roborovski] = 180,
        [HamsterSpecies.ChineseStriped] = 200,
    };

    public static HealthResult Compute(HealthQuery q)
    {
        double circumferenceKm = Math.PI * q.WheelDiameterMm / 1_000_000;
        double rpm = q.TargetSpeedKmh / 60 / circumferenceKm;
        double hours = q.DailyDistanceKm / q.TargetSpeedKmh;

        double minD = MinDiameter[q.Species];
        var risk = q.WheelDiameterMm < minD - 30 ? SpineRiskLevel.High
                      : q.WheelDiameterMm < minD ? SpineRiskLevel.Medium
                      : q.WheelDiameterMm < minD + 20 ? SpineRiskLevel.Low
                      : SpineRiskLevel.None;

        bool pl = Strings.Current.Language == AppLanguage.Polish;
        string rec = risk switch
        {
            SpineRiskLevel.High   => pl ? $"Za małe! Min. dla {q.Species} to {minD} mm."
                                        : $"Too small! Min for {q.Species} is {minD} mm.",
            SpineRiskLevel.Medium => pl ? $"Poniżej zalecanego min. {minD} mm."
                                        : $"Below recommended min {minD} mm.",
            SpineRiskLevel.Low    => pl ? "Akceptowalne, ale większe jest lepsze."
                                        : "Acceptable, but larger is better.",
            _                     => pl ? "Optymalnie dla gatunku."
                                        : "Optimal size for species.",
        };

        return new(Math.Round(rpm, 1), Math.Round(hours, 2), risk, rec);
    }
}
