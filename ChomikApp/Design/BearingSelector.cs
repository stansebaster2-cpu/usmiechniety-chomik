namespace ChomikEngine.Design;

public record BearingQuery(
    double WheelDiameterMm,
    double HamsterMassGrams = 150
);

public static class BearingSelector
{
    public static BearingSpec Select(BearingQuery q)
    {
        if (q.WheelDiameterMm < 200)
            return BearingCatalog.Get("MR84ZZ");
        if (q.WheelDiameterMm >= 280 || q.HamsterMassGrams > 300)
            return BearingCatalog.Get("608ZZ");
        if (q.WheelDiameterMm >= 250)
            return BearingCatalog.Get("626ZZ");
        if (q.WheelDiameterMm >= 220)
            return BearingCatalog.Get("625ZZ");
        return BearingCatalog.Get("624ZZ");
    }

    public static double PrintedAxleDiameterMm(BearingSpec b) =>
        b.BoreDiameterMm + 0.2;

    public static double SocketDiameterMm(BearingSpec b) =>
        b.OuterDiameterMm + 0.1;
}
