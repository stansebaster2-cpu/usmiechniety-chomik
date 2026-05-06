namespace ChomikEngine.Design;

public record BearingSpec(
    string Id,
    double OuterDiameterMm,
    double WidthMm,
    double BoreDiameterMm,
    int MaxRpm,
    string Notes
);

public static class BearingCatalog
{
    public static readonly IReadOnlyList<BearingSpec> All =
    [
        new("624ZZ",  13, 5, 4, 30000, "Default - wheels < 250 mm"),
        new("625ZZ",  16, 5, 5, 26000, "Wheels 220-270 mm"),
        new("626ZZ",  19, 6, 6, 22000, "Wheels 250-300 mm"),
        new("MR84ZZ",  8, 4, 4, 40000, "Ultra-small wheels < 200 mm"),
        new("608ZZ",  22, 7, 8, 18000, "Heavy wheels >= 280 mm"),
    ];

    public static BearingSpec Get(string id) =>
        All.First(b => b.Id == id);
}
