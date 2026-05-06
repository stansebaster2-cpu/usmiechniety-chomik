namespace ChomikEngine.Design;

public enum MountType
{
    FloorStand,
    CageHook,
    MagneticClip
}

public record BaseQuery(
    double WheelDiameterMm,
    BearingSpec Bearing,
    MountType Mount = MountType.FloorStand,
    double StandHeightMm = 120,
    double HookWireMm = 4,
    bool Screwless = true
);

public record BaseParameters(
    double BearingSocketDiameterMm,
    double BearingSocketDepthMm,
    double AxleDiameterMm,
    double SnapFitOverlapMm,
    int BearingCount,
    double BearingCircleRadiusMm,
    double StandHeightMm,
    double StandBaseDiameterMm,
    double StandWallThicknessMm,
    double HookSlotWidthMm,
    double HookSlotDepthMm,
    double HookArmLengthMm,
    string ScadParams
);

public static class BaseDesigner
{
    public static BaseParameters Design(BaseQuery q)
    {
        var socketD = BearingSelector.SocketDiameterMm(q.Bearing);
        var axleD = BearingSelector.PrintedAxleDiameterMm(q.Bearing);
        double bR = q.WheelDiameterMm / 2 * 0.864;
        double baseDia = bR * 2 + socketD + 12;

        string scad = $"""
loz_D         = {socketD:F1};
loz_W         = {q.Bearing.WidthMm:F1};
axle_print_D  = {axleD:F1};
bearing_R     = {bR:F1};
stand_h       = {q.StandHeightMm:F0};
base_type     = \"{q.Mount.ToString().ToLowerInvariant()}\";
hook_wire_D   = {q.HookWireMm:F0};
snap_overlap  = 0.4;
""";

        return new BaseParameters(
            BearingSocketDiameterMm: socketD,
            BearingSocketDepthMm: q.Bearing.WidthMm + 0.5,
            AxleDiameterMm: axleD,
            SnapFitOverlapMm: 0.4,
            BearingCount: 3,
            BearingCircleRadiusMm: bR,
            StandHeightMm: q.StandHeightMm,
            StandBaseDiameterMm: baseDia,
            StandWallThicknessMm: 3.2,
            HookSlotWidthMm: q.HookWireMm + 0.5,
            HookSlotDepthMm: q.HookWireMm * 1.8,
            HookArmLengthMm: 40,
            ScadParams: scad
        );
    }
}
