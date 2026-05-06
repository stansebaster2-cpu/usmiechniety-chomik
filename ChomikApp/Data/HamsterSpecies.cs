using ChomikApp.Localization;

namespace ChomikEngine.Data;

public enum SpeciesId
{
    Syrian,
    Djungarian,
    Roborovski,
    Chinese,
    Custom
}

public class HamsterSpecies
{
    public SpeciesId Id { get; init; }
    public string NamePl { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public float BodyLengthMinMm { get; init; }
    public float BodyLengthMaxMm { get; init; }
    public float MinWheelDiameterMm { get; init; }
    public float RecommendedDiamMm { get; init; }
    public float MaxDiameterMm { get; init; }
    public float RecommendedTrackMm { get; init; }
    public string HealthNote { get; init; } = string.Empty;
    public string HealthNoteEn { get; init; } = string.Empty;

    public string DisplayName =>
        Strings.Current.Language == AppLanguage.Polish ? NamePl : NameEn;

    public string CurrentHealthNote =>
        Strings.Current.Language == AppLanguage.Polish ? HealthNote : HealthNoteEn;

    public WheelDefaults GetDefaults() => new()
    {
        DiameterMm = RecommendedDiamMm,
        TrackWidthMm = RecommendedTrackMm,
        SurfaceType = SurfaceType.Ribbed,
        MountType = MountType.Freestanding
    };
}

public record WheelDefaults
{
    public float DiameterMm { get; init; }
    public float TrackWidthMm { get; init; }
    public SurfaceType SurfaceType { get; init; }
    public MountType MountType { get; init; }
}

public enum SurfaceType { Smooth, Ribbed, GridTexture }
public enum MountType { Freestanding, Hanging }
