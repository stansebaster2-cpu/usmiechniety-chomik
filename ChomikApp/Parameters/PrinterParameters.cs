using System.Text.Json.Serialization;

namespace ChomikEngine.Parameters;

public class PrinterParameters
{
    [JsonPropertyName("bed_x_mm")] public float BedXMm { get; set; } = 250f;
    [JsonPropertyName("bed_y_mm")] public float BedYMm { get; set; } = 210f;
    [JsonPropertyName("bed_z_mm")] public float BedZMm { get; set; } = 210f;
    [JsonPropertyName("name")] public string Name { get; set; } = "Prusa MK4";

    public bool FitsOnBed(float modelDiamMm, float modelHeightMm)
        => modelDiamMm <= BedXMm && modelDiamMm <= BedYMm && modelHeightMm <= BedZMm;
}
