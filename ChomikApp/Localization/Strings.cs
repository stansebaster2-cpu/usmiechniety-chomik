using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChomikApp.Localization;

public enum AppLanguage { Polish, English }

public class Strings : INotifyPropertyChanged
{
    static Strings? _instance;
    public static Strings Current => _instance ??= new Strings();

    public AppLanguage Language { get; private set; } = AppLanguage.Polish;

    public void SetLanguage(AppLanguage lang)
    {
        Language = lang;
        LanguageChanged?.Invoke();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    public event Action? LanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    string T(string pl, string en) => Language == AppLanguage.Polish ? pl : en;

    public string AppTitle           => T("Uśmiechnięty Chomik", "Happy Hamster");
    public string AppSubtitle        => T("Generator Kołowrotka", "Hamster Wheel Generator");
    public string BtnGenerate        => T("⚙ Generuj", "⚙ Generate");
    public string BtnTestSocket      => T("🔩 Test gniazda", "🔩 Test socket");
    public string BtnExport          => T("💾 Eksportuj", "💾 Export");
    public string BtnLoadStl         => T("📂 Wczytaj STL", "📂 Load STL");
    public string BtnSavePreset      => T("💾 Preset", "💾 Preset");
    public string BtnLoadPreset      => T("📂 Preset", "📂 Preset");
    public string LblSpecies         => T("Gatunek chomika", "Hamster species");
    public string LblDiameter        => T("Średnica koła [mm]", "Wheel diameter [mm]");
    public string LblTrackWidth      => T("Szerokość toru [mm]", "Track width [mm]");
    public string LblWallThickness   => T("Grubość ścianki [mm]", "Wall thickness [mm]");
    public string LblSurface         => T("Powierzchnia biegowa", "Running surface");
    public string LblGrooveAngle     => T("Kąt V-groove [°]", "V-groove angle [°]");
    public string LblSegments        => T("Liczba segmentów", "Segment count");
    public string LblMountType       => T("Typ montażu", "Mount type");
    public string LblBaseType        => T("Typ podstawy", "Base type");
    public string BaseTypeBearingTrack => T("Bieżnia łożyskowa", "Bearing track");
    public string BaseTypeHanging      => T("Podwieszane koło na śrubie", "Hanging wheel on screw");
    public string LblBaseWidth       => T("Szerokość podstawy [mm]", "Base width [mm]");
    public string LblBaseLength      => T("Długość podstawy [mm]", "Base length [mm]");
    public string LblBearing         => T("Łożysko", "Bearing");
    public string LblBearingCount    => T("Liczba łożysk / grzybków", "Bearing / mushroom count");
    public string LblBearingRadius   => T("Promień rozmieszczenia [mm]", "Placement radius [mm]");
    public string LblTolerance       => T("Tolerancja gniazda press-fit [mm]", "Press-fit socket tolerance [mm]");
    public string LblSnapFitCover    => T("Snap-fit pokrywa", "Snap-fit cover");
    public string LblVSlot           => T("Prowadnica V-slot (bieżnia)", "V-slot rail (track)");
    public string LblVSlotType       => T("Profil V-slot", "V-slot profile");
    public string VSlot2020          => T("V-slot 20×20mm (standardowy)", "V-slot 20×20mm (standard)");
    public string VSlot2040          => T("V-slot 20×40mm (szeroki)", "V-slot 20×40mm (wide)");
    public string VSlotCustom        => T("Własny profil", "Custom profile");
    public string VSlotHint          => T("Zalecane łożysko do V-slot: 608ZZ lub 688ZZ",
                                          "Recommended bearings for V-slot: 608ZZ or 688ZZ");
    public string LblWheelWidthOnTrack => T("Szerokość koła na bieżni [mm]", "Wheel width on track [mm]");
    public string LblPrinter         => T("Drukarka — pole robocze", "Printer — build volume");
    public string LblBedX            => T("Pole robocze X [mm]", "Bed X [mm]");
    public string LblBedY            => T("Pole robocze Y [mm]", "Bed Y [mm]");
    public string LblBedZ            => T("Wysokość Z [mm]", "Z height [mm]");
    public string LblMaterial        => T("Materiał", "Material");
    public string MaterialHint       => T("💡 Zalecany materiał: PETG — elastyczny, odporny na chomicze żucie i wilgoć.",
                                          "💡 Recommended material: PETG — flexible, resistant to chewing and moisture.");
    public string SecPrinter         => T("Drukarka", "Printer");
    public string SecBaseGenerator   => T("Generator podstawy", "Base generator");
    public string SecWheel           => T("Koło", "Wheel");
    public string ShowWheel          => T("Pokaż koło", "Show wheel");
    public string ShowBase           => T("Pokaż podstawę", "Show base");
    public string StatusReady        => T("Gotowy.", "Ready.");
    public string StatusGenerating   => T("Generowanie modelu...", "Generating model...");
    public string StatusExported     => T("Wyeksportowano: ", "Exported: ");
    public string TabWheel           => T("🎡 Koło", "🎡 Wheel");
    public string TabBase            => T("🔧 Podstawa", "🔧 Base");
    public string TabPrinter         => T("🖨 Drukarka", "🖨 Printer");
    public string SpeciesDialogTitle => T("Wybierz gatunek chomika", "Select hamster species");
    public string SpeciesDialogBody  => T(
        "Wybierz gatunek, a aplikacja ustawi zalecane parametry koła.\nMożesz je potem dowolnie modyfikować.",
        "Select species and the app will set safe wheel parameters.\nYou can modify them freely afterwards.");
    public string SpeciesWarning     => T("⚠ Uwaga zdrowotna", "⚠ Health note");
    public string BtnApplyDefaults   => T("Zastosuj zalecane parametry", "Apply recommended parameters");
    public string BtnCustomize       => T("Ustaw ręcznie", "Set manually");
    public string WarnTooSmall       => T(
        "⚠ Koło może być za małe dla wybranego gatunku.\nZalecane minimum: ",
        "⚠ Wheel may be too small for selected species.\nRecommended minimum: ");
    public string WarnTooBig         => T(
        "⚠ Model może nie zmieścić się na stole drukarki.",
        "⚠ Model may not fit on the printer bed.");
    public string SurfaceSmooth      => T("Gładka", "Smooth");
    public string SurfaceRibbed      => T("Żeberkowana (zalecana)", "Ribbed (recommended)");
    public string SurfaceGrid        => T("Siatka", "Grid texture");
    public string MountFreestanding  => T("Wolnostojący (podstawa)", "Freestanding (base)");
    public string MountHanging       => T("Zawieszany (kratka)", "Hanging (cage bars)");
    public string MeshInfoSuffix     => T("trójkątów", "triangles");
    public string CameraHints        => T("LPM: orbituj  |  PPM: pan  |  Scroll: zoom  |  Dbl.klik: dopasuj",
                                          "LMB: orbit  |  RMB: pan  |  Scroll: zoom  |  Dbl-click: fit");
    public string SpeciesRecLabel    => T("Zalecane koło:", "Recommended wheel:");

    public string AboutTitle         => T("O programie", "About");
    public string BtnAbout           => T("ℹ", "ℹ");
    public string BtnClose           => T("Zamknij", "Close");
    public string AboutAuthorLabel   => T("Autor", "Author");
    public string AboutLicenseHeader => T("Licencja MIT — oprogramowanie otwarte", "MIT License — open source");
    public string AboutLicenseBody   => T(
        "Program jest w pełni otwartoźródłowy i bezpłatny. Możesz go swobodnie używać, modyfikować i dystrybuować — również w celach komercyjnych — pod warunkiem zachowania informacji o licencji i autorze.\n\nProgram powstał z troski o dobrostan chomików i z przekonania, że odpowiednio duże, dobrze zaprojektowane koło może realnie wydłużyć życie i poprawić zdrowie kręgosłupa Twojego zwierzęcia. Budujemy świadomość — dobre koło to nie luksus, to potrzeba każdego chomika.\n\nCopyright © 2026 Sebastian Stańczykowski",
        "This software is fully open-source and free. You may freely use, modify and distribute it — including for commercial purposes — as long as the license and author notice is preserved.\n\nThis program was created out of care for hamster welfare and the belief that a properly sized, well-designed wheel can genuinely extend your hamster's life and protect its spine health. We are building awareness — a good wheel is not a luxury, it is every hamster's need.\n\nCopyright © 2026 Sebastian Stańczykowski");
    public string AboutGithubLabel   => T("Kod źródłowy (GitHub)", "Source code (GitHub)");
    public string AboutGithubUrl     => "https://github.com/stansebaster2-cpu/usmiechniety-chomik";
    public string AboutBuyMeACoffee  => T("Wesprzyj projekt", "Support the project");
    public string AboutBuyMeACoffeeUrl => "https://buymeacoffee.com/stansebaster";
    public string AboutPhotoCaption  => T("Mój chomik 🐹", "My hamster 🐹");
    public string AboutClaude        => T(
        "Stworzony przy pomocy Claude (Anthropic) — asystenta AI, który pomógł w projektowaniu i implementacji.",
        "Built with the help of Claude (Anthropic) — an AI assistant that helped design and implement this app.");
}
