using System.Text.Json.Serialization;
using ChomikEngine.Data;

namespace ChomikEngine.Parameters;

public class WheelParameters
{
    [JsonPropertyName("diameter_mm")] public float DiameterMm { get; set; } = 280f;
    [JsonPropertyName("track_width_mm")] public float TrackWidthMm { get; set; } = 65f;
    [JsonPropertyName("wall_thickness_mm")] public float WallThicknessMm { get; set; } = 2.5f;
    [JsonPropertyName("groove_angle_deg")] public float GrooveAngleDeg { get; set; } = 45f;
    [JsonPropertyName("segments")] public int Segments { get; set; } = 240;
    [JsonPropertyName("surface_type")] public SurfaceType Surface { get; set; } = SurfaceType.Ribbed;
    [JsonPropertyName("rib_height_mm")] public float RibHeightMm { get; set; } = 0.8f;
    [JsonPropertyName("rib_spacing_mm")] public float RibSpacingMm { get; set; } = 4f;
    [JsonPropertyName("mount_type")] public MountType Mount { get; set; } = MountType.Freestanding;
}
