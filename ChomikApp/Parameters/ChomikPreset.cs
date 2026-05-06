using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChomikEngine.Data;

namespace ChomikEngine.Parameters;

public class ChomikPreset
{
    [JsonPropertyName("name")] public string Name { get; set; } = "Mój preset";
    [JsonPropertyName("species")] public SpeciesId Species { get; set; } = SpeciesId.Syrian;
    [JsonPropertyName("wheel")] public WheelParameters Wheel { get; set; } = new();
    [JsonPropertyName("base")] public BaseParameters Base { get; set; } = new();
    [JsonPropertyName("printer")] public PrinterParameters Printer { get; set; } = new();
    [JsonPropertyName("material")] public string Material { get; set; } = "PETG";
    [JsonPropertyName("notes")] public string Notes { get; set; } = string.Empty;

    static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public void Save(string path)
        => File.WriteAllText(path, JsonSerializer.Serialize(this, _opts));

    public static ChomikPreset? Load(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<ChomikPreset>(File.ReadAllText(path), _opts);
    }

    public static ChomikPreset FromSpecies(HamsterSpecies species)
    {
        var def = species.GetDefaults();
        var bearing = BearingDatabase.RecommendForWheel(def.DiameterMm);
        return new ChomikPreset
        {
            Name = species.NamePl,
            Species = species.Id,
            Wheel = new()
            {
                DiameterMm = def.DiameterMm,
                TrackWidthMm = def.TrackWidthMm,
                Surface = def.SurfaceType,
                Mount = def.MountType,
            },
            Base = new()
            {
                BearingCode = bearing.Code,
                BearingCount = 3,
                BearingRadiusMm = def.DiameterMm / 2f * 0.65f,
                AxleDiameterMm = bearing.InnerDiamMm + 0.2f,
            }
        };
    }
}
