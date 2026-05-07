using System.Text.Json.Serialization;
using ChomikEngine.Data;

namespace ChomikEngine.Parameters;

public enum DiscPattern { Full, Spoked, Cross }

public class WheelParameters
{
    [JsonPropertyName("diameter_mm")]      public float DiameterMm       { get; set; } = 280f;
    [JsonPropertyName("track_width_mm")]   public float TrackWidthMm     { get; set; } = 65f;
    [JsonPropertyName("wall_thickness_mm")]public float WallThicknessMm  { get; set; } = 2.5f;
    [JsonPropertyName("segments")]         public int   Segments          { get; set; } = 240;
    [JsonPropertyName("surface_type")]     public SurfaceType Surface     { get; set; } = SurfaceType.Ribbed;
    [JsonPropertyName("step_height_mm")]   public float StepHeightMm     { get; set; } = 1.2f;
    [JsonPropertyName("step_spacing_mm")]  public float StepSpacingMm    { get; set; } = 5f;
    [JsonPropertyName("disc_pattern")]     public DiscPattern DiscPattern { get; set; } = DiscPattern.Spoked;
    [JsonPropertyName("spoke_count")]      public int   SpokeCount        { get; set; } = 6;
    [JsonPropertyName("spoke_width_mm")]   public float SpokeWidthMm     { get; set; } = 8f;
    [JsonPropertyName("mount_type")]       public MountType Mount         { get; set; } = MountType.Freestanding;
    // kept for preset compatibility
    [JsonPropertyName("groove_angle_deg")] public float GrooveAngleDeg   { get; set; } = 45f;
    [JsonPropertyName("rib_height_mm")]    public float RibHeightMm      { get; set; } = 0.8f;
    [JsonPropertyName("rib_spacing_mm")]   public float RibSpacingMm     { get; set; } = 4f;
}
