namespace ChomikEngine.Data;

public enum VSlotType { V2020, V2040, Custom }

public class VSlotProfile
{
    public VSlotType Type { get; set; } = VSlotType.V2020;
    public float WidthMm { get; set; } = 20f;
    public float HeightMm { get; set; } = 20f;
    public float GrooveWidthMm { get; set; } = 6f;
    public float GrooveDepthMm { get; set; } = 1.8f;
    public string Label => Type == VSlotType.Custom
        ? $"Custom {WidthMm}×{HeightMm}mm"
        : Type == VSlotType.V2020 ? "V-slot 20×20mm" : "V-slot 20×40mm";

    public static VSlotProfile V2020 => new()
    {
        Type = VSlotType.V2020,
        WidthMm = 20,
        HeightMm = 20,
        GrooveWidthMm = 6,
        GrooveDepthMm = 1.8f
    };

    public static VSlotProfile V2040 => new()
    {
        Type = VSlotType.V2040,
        WidthMm = 20,
        HeightMm = 40,
        GrooveWidthMm = 6,
        GrooveDepthMm = 1.8f
    };
}
