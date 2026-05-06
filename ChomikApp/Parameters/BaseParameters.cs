using System.Text.Json.Serialization;
using ChomikEngine.Data;

namespace ChomikEngine.Parameters;

public enum BaseType
{
    BearingTrack = 0,
    HangingScrew = 1
}

public class BaseParameters
{
    [JsonPropertyName("base_type")] public BaseType Type { get; set; } = BaseType.BearingTrack;
    [JsonPropertyName("base_width_mm")] public float BaseWidthMm { get; set; } = 320f;
    [JsonPropertyName("base_length_mm")] public float BaseLengthMm { get; set; } = 120f;
    [JsonPropertyName("bearing_code")] public string BearingCode { get; set; } = "624ZZ";
    [JsonPropertyName("bearing_count")] public int BearingCount { get; set; } = 3;
    [JsonPropertyName("bearing_radius_mm")] public float BearingRadiusMm { get; set; } = 101.5f;
    [JsonPropertyName("socket_tolerance_mm")] public float SocketToleranceMm { get; set; } = 0.1f;
    [JsonPropertyName("axle_diameter_mm")] public float AxleDiameterMm { get; set; } = 4.2f;
    [JsonPropertyName("snap_fit_cover")] public bool SnapFitCover { get; set; } = true;
    [JsonPropertyName("vslot_enabled")] public bool VSlotEnabled { get; set; } = false;
    [JsonPropertyName("vslot_type")] public VSlotType VSlotType { get; set; } = VSlotType.V2020;
    [JsonPropertyName("vslot_custom_w")] public float VSlotCustomWidthMm { get; set; } = 20f;
    [JsonPropertyName("vslot_custom_h")] public float VSlotCustomHeightMm { get; set; } = 20f;

    public VSlotProfile GetVSlotProfile() => VSlotType switch
    {
        VSlotType.V2020 => VSlotProfile.V2020,
        VSlotType.V2040 => VSlotProfile.V2040,
        VSlotType.Custom => new VSlotProfile
        {
            Type = VSlotType.Custom,
            WidthMm = VSlotCustomWidthMm,
            HeightMm = VSlotCustomHeightMm
        },
        _ => VSlotProfile.V2020
    };

    public Bearing? GetBearing()
        => BearingDatabase.All.FirstOrDefault(b => b.Code == BearingCode);
}
