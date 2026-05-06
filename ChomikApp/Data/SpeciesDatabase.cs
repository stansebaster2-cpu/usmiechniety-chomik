namespace ChomikEngine.Data;

public static class SpeciesDatabase
{
    public static IReadOnlyList<HamsterSpecies> All { get; } = new List<HamsterSpecies>
    {
        new()
        {
            Id = SpeciesId.Syrian,
            NamePl = "Chomik syryjski (złoty)",
            NameEn = "Syrian (golden) hamster",
            BodyLengthMinMm = 130,
            BodyLengthMaxMm = 180,
            MinWheelDiameterMm = 250,
            RecommendedDiamMm = 300,
            MaxDiameterMm = 400,
            RecommendedTrackMm = 75,
            HealthNote   = "Minimalna średnica 25 cm — mniejsze koło powoduje wygięcie kręgosłupa.",
            HealthNoteEn = "Minimum diameter 25 cm — smaller wheel causes spine curvature."
        },
        new()
        {
            Id = SpeciesId.Djungarian,
            NamePl = "Chomik dżungar",
            NameEn = "Djungarian hamster",
            BodyLengthMinMm = 70,
            BodyLengthMaxMm = 110,
            MinWheelDiameterMm = 200,
            RecommendedDiamMm = 230,
            MaxDiameterMm = 300,
            RecommendedTrackMm = 55,
            HealthNote   = "Minimalna średnica 20 cm. Mniejsze koła szkodzą kręgosłupowi.",
            HealthNoteEn = "Minimum diameter 20 cm. Smaller wheels damage the spine."
        },
        new()
        {
            Id = SpeciesId.Roborovski,
            NamePl = "Chomik Roborovskiego",
            NameEn = "Roborovski hamster",
            BodyLengthMinMm = 45,
            BodyLengthMaxMm = 55,
            MinWheelDiameterMm = 150,
            RecommendedDiamMm = 180,
            MaxDiameterMm = 250,
            RecommendedTrackMm = 40,
            HealthNote   = "Minimalna średnica 15 cm. Najmniejsi — ale też potrzebują właściwego koła!",
            HealthNoteEn = "Minimum diameter 15 cm. Smallest species — but still need a proper wheel!"
        },
        new()
        {
            Id = SpeciesId.Chinese,
            NamePl = "Chomik chiński",
            NameEn = "Chinese hamster",
            BodyLengthMinMm = 100,
            BodyLengthMaxMm = 130,
            MinWheelDiameterMm = 210,
            RecommendedDiamMm = 250,
            MaxDiameterMm = 320,
            RecommendedTrackMm = 60,
            HealthNote   = "Minimalna średnica 21 cm. Smukłe ciało — ważna też odpowiednia szerokość toru.",
            HealthNoteEn = "Minimum diameter 21 cm. Slender body — track width matters too."
        },
        new()
        {
            Id = SpeciesId.Custom,
            NamePl = "Własny / inny gatunek",
            NameEn = "Custom / other species",
            BodyLengthMinMm = 0,
            BodyLengthMaxMm = 999,
            MinWheelDiameterMm = 100,
            RecommendedDiamMm = 250,
            MaxDiameterMm = 500,
            RecommendedTrackMm = 60,
            HealthNote   = "Ustaw parametry ręcznie. Upewnij się że koło jest odpowiednio duże dla Twojego zwierzęcia.",
            HealthNoteEn = "Set parameters manually. Make sure the wheel is large enough for your animal."
        }
    };

    public static HamsterSpecies? Get(SpeciesId id)
        => All.FirstOrDefault(s => s.Id == id);
}
