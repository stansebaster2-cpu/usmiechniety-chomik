namespace ChomikEngine.Data;

public class Bearing
{
    public string Code { get; init; } = string.Empty;
    public float OuterDiamMm { get; init; }
    public float InnerDiamMm { get; init; }
    public float WidthMm { get; init; }
    public string Notes { get; init; } = string.Empty;
    public bool SuitableVSlot { get; init; }
    public float? VSlotSizeMm { get; init; }
}

public static class BearingDatabase
{
    public static IReadOnlyList<Bearing> All { get; } = new List<Bearing>
    {
        new() { Code = "624ZZ", OuterDiamMm = 13, InnerDiamMm = 4, WidthMm = 5, SuitableVSlot = false, Notes = "Standard — grzybek press-fit Ø13mm" },
        new() { Code = "625ZZ", OuterDiamMm = 16, InnerDiamMm = 5, WidthMm = 5, SuitableVSlot = false, Notes = "Szerszy wewnątrz, do grubszej osi" },
        new() { Code = "626ZZ", OuterDiamMm = 19, InnerDiamMm = 6, WidthMm = 6, SuitableVSlot = false, Notes = "Większy, do cięższych kół" },
        new() { Code = "608ZZ", OuterDiamMm = 22, InnerDiamMm = 8, WidthMm = 7, SuitableVSlot = true, VSlotSizeMm = 20, Notes = "Do V-slot 20×20mm — standardowe do prowadnic" },
        new() { Code = "623ZZ", OuterDiamMm = 10, InnerDiamMm = 3, WidthMm = 4, SuitableVSlot = false, Notes = "Mały — do miniaturowych osi" },
        new() { Code = "688ZZ", OuterDiamMm = 16, InnerDiamMm = 8, WidthMm = 5, SuitableVSlot = true, VSlotSizeMm = 20, Notes = "Do V-slot 20×20mm — alternatywa dla 608ZZ" },
        new() { Code = "6900ZZ", OuterDiamMm = 22, InnerDiamMm = 10, WidthMm = 6, SuitableVSlot = true, VSlotSizeMm = 40, Notes = "Do V-slot 20×40mm — profil szeroki" },
        new() { Code = "6001ZZ", OuterDiamMm = 28, InnerDiamMm = 12, WidthMm = 8, SuitableVSlot = false, Notes = "Duże koło — oś Ø12mm" },
        new() { Code = "6002ZZ", OuterDiamMm = 32, InnerDiamMm = 15, WidthMm = 9, SuitableVSlot = false, Notes = "Bardzo duże koło — oś Ø15mm" },
        new() { Code = "MR104ZZ", OuterDiamMm = 10, InnerDiamMm = 4, WidthMm = 4, SuitableVSlot = false, Notes = "Mini — do małych kołowrotków" }
    };

    public static IEnumerable<Bearing> ForVSlot(float vslotMm)
        => All.Where(b => b.SuitableVSlot && b.VSlotSizeMm.HasValue && MathF.Abs(b.VSlotSizeMm.Value - vslotMm) < 0.1f);

    public static Bearing RecommendForWheel(float wheelDiamMm) => wheelDiamMm switch
    {
        <= 200 => All.First(b => b.Code == "624ZZ"),
        <= 280 => All.First(b => b.Code == "625ZZ"),
        _ => All.First(b => b.Code == "626ZZ"),
    };
}
