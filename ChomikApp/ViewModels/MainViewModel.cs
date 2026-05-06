using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChomikApp.Localization;
using ChomikEngine.Data;
using ChomikEngine.Export;
using ChomikEngine.Geometry;
using ChomikEngine.Models;
using ChomikEngine.Parameters;
using Serilog;
using DesignHamsterSpecies  = ChomikEngine.Design.HamsterSpecies;
using DesignBaseQuery       = ChomikEngine.Design.BaseQuery;
using DesignBearingQuery    = ChomikEngine.Design.BearingQuery;
using DesignSpineRisk       = ChomikEngine.Design.SpineRiskLevel;
using DesignHealthQuery     = ChomikEngine.Design.HealthQuery;
using DesignHealthCalc      = ChomikEngine.Design.HamsterHealthCalc;
using DesignBearingSelector = ChomikEngine.Design.BearingSelector;
using DesignBaseDesigner    = ChomikEngine.Design.BaseDesigner;

namespace ChomikApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private WheelParameters _wheelParams = new();
    [ObservableProperty] private BaseParameters _baseParams = new();
    [ObservableProperty] private PrinterParameters _printerParams = new();
    [ObservableProperty] private SpeciesId _selectedSpecies = SpeciesId.Syrian;
    [ObservableProperty] private string _statusText = Strings.Current.StatusReady;
    [ObservableProperty] private Mesh? _currentMesh;
    [ObservableProperty] private string _meshInfo = "—";
    [ObservableProperty] private bool _showWheel = true;
    [ObservableProperty] private bool _showBase = true;
    [ObservableProperty] private string _healthWarning = string.Empty;
    [ObservableProperty] private bool _hasHealthWarning;
    [ObservableProperty] private bool _hasPrinterWarning;
    [ObservableProperty] private string _healthDetail = string.Empty;
    [ObservableProperty] private bool _hasHealthDetail;

    public IReadOnlyList<HamsterSpecies> AllSpecies => SpeciesDatabase.All;
    public IReadOnlyList<Bearing> AllBearings => BearingDatabase.All;
    public string[] ExportFormats => new[] { "STL Binary", "STL ASCII", "OBJ", "PLY" };
    public int SelectedFormatIndex { get; set; } = 0;
    public string[] Materials => new[] { "PETG", "PLA", "ASA", "ABS", "PETG-CF" };

    public int SurfaceTypeIndex
    {
        get => (int)WheelParams.Surface;
        set { WheelParams.Surface = (SurfaceType)value; OnPropertyChanged(); }
    }

    public int VSlotTypeIndex
    {
        get => (int)BaseParams.VSlotType;
        set { BaseParams.VSlotType = (VSlotType)value; OnPropertyChanged(); }
    }

    public int BaseTypeIndex
    {
        get => (int)BaseParams.Type;
        set
        {
            BaseParams.Type = (BaseType)value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsBearingTrack));
            OnPropertyChanged(nameof(IsHangingScrew));
            if (BaseParams.Type == BaseType.HangingScrew)
                SyncWheelFromHangingBase();
        }
    }

    public bool IsBearingTrack => BaseParams.Type == BaseType.BearingTrack;
    public bool IsHangingScrew => BaseParams.Type == BaseType.HangingScrew;

    public float BaseWidthMm
    {
        get => BaseParams.BaseWidthMm;
        set
        {
            if (Math.Abs(BaseParams.BaseWidthMm - value) < 0.001f) return;
            BaseParams.BaseWidthMm = value;
            OnPropertyChanged();
            if (BaseParams.Type == BaseType.HangingScrew)
                SyncWheelFromHangingBase();
        }
    }

    public float BaseLengthMm
    {
        get => BaseParams.BaseLengthMm;
        set
        {
            if (Math.Abs(BaseParams.BaseLengthMm - value) < 0.001f) return;
            BaseParams.BaseLengthMm = value;
            OnPropertyChanged();
            if (BaseParams.Type == BaseType.HangingScrew)
                SyncWheelFromHangingBase();
        }
    }

    private void SyncWheelFromHangingBase()
    {
        // Hanging-on-screw: wheel diameter follows base width (minus mount clearance);
        // wheel track width follows base length.
        const float MountClearanceMm = 30f;
        float newDiameter = MathF.Max(100f, BaseParams.BaseWidthMm - MountClearanceMm);
        float newTrack = MathF.Max(30f, BaseParams.BaseLengthMm - 10f);
        if (Math.Abs(WheelParams.DiameterMm - newDiameter) > 0.01f)
        {
            WheelParams.DiameterMm = newDiameter;
            OnPropertyChanged(nameof(WheelParams));
        }
        if (Math.Abs(WheelParams.TrackWidthMm - newTrack) > 0.01f)
        {
            WheelParams.TrackWidthMm = newTrack;
            OnPropertyChanged(nameof(WheelParams));
        }
        Validate();
    }

    public string PrinterWarning => Strings.Current.WarnTooBig;

    partial void OnWheelParamsChanged(WheelParameters value)
    {
        Validate();
        OnPropertyChanged(nameof(SurfaceTypeIndex));
    }

    partial void OnBaseParamsChanged(BaseParameters value)
    {
        OnPropertyChanged(nameof(VSlotTypeIndex));
        OnPropertyChanged(nameof(BaseTypeIndex));
        OnPropertyChanged(nameof(IsBearingTrack));
        OnPropertyChanged(nameof(IsHangingScrew));
        OnPropertyChanged(nameof(BaseWidthMm));
        OnPropertyChanged(nameof(BaseLengthMm));
    }
    partial void OnSelectedSpeciesChanged(SpeciesId value) => Validate();
    partial void OnPrinterParamsChanged(PrinterParameters value) => Validate();
    partial void OnShowWheelChanged(bool value) => Validate();
    partial void OnShowBaseChanged(bool value) => Validate();
    partial void OnHasPrinterWarningChanged(bool value) => OnPropertyChanged(nameof(PrinterWarning));

    public float BedXMm
    {
        get => PrinterParams.BedXMm;
        set { PrinterParams.BedXMm = value; OnPropertyChanged(); Validate(); }
    }
    public float BedYMm
    {
        get => PrinterParams.BedYMm;
        set { PrinterParams.BedYMm = value; OnPropertyChanged(); Validate(); }
    }
    public float BedZMm
    {
        get => PrinterParams.BedZMm;
        set { PrinterParams.BedZMm = value; OnPropertyChanged(); Validate(); }
    }

    public float SocketToleranceMm
    {
        get => MathF.Round(BaseParams.SocketToleranceMm, 1);
        set { BaseParams.SocketToleranceMm = MathF.Round(value, 1); OnPropertyChanged(); }
    }

    private void Validate()
    {
        var species = SpeciesDatabase.Get(SelectedSpecies);
        if (species is null) return;

        bool tooSmall = WheelParams.DiameterMm < species.MinWheelDiameterMm;
        HasHealthWarning = tooSmall;

        if (SelectedSpecies != SpeciesId.Custom)
        {
            var query = new DesignHealthQuery(WheelParams.DiameterMm, MapToDesignSpecies(SelectedSpecies));
            var result = DesignHealthCalc.Compute(query);
            string riskEmoji = result.SpineRisk switch
            {
                DesignSpineRisk.High   => "🔴",
                DesignSpineRisk.Medium => "🟠",
                DesignSpineRisk.Low    => "🟡",
                _                      => "🟢"
            };
            HealthDetail = $"{riskEmoji} {result.Recommendation}  |  ~{result.Rpm} RPM  |  ~{result.DurationHours}h/dobę";
            HasHealthDetail = true;
            HealthWarning = tooSmall
                ? Strings.Current.WarnTooSmall + $"{species.MinWheelDiameterMm:0} mm\n{result.Recommendation}"
                : string.Empty;
        }
        else
        {
            HealthDetail = string.Empty;
            HasHealthDetail = false;
            HealthWarning = tooSmall
                ? Strings.Current.WarnTooSmall + $"{species.MinWheelDiameterMm:0} mm"
                : string.Empty;
        }

        HasPrinterWarning = !PrinterParams.FitsOnBed(WheelParams.DiameterMm, WheelParams.TrackWidthMm + 20f);
    }

    private static DesignHamsterSpecies MapToDesignSpecies(SpeciesId id) => id switch
    {
        SpeciesId.Syrian     => DesignHamsterSpecies.Syrian,
        SpeciesId.Roborovski => DesignHamsterSpecies.Roborovski,
        SpeciesId.Chinese    => DesignHamsterSpecies.ChineseStriped,
        _                    => DesignHamsterSpecies.Dwarf,
    };

    [RelayCommand]
    public async Task GenerateModelAsync()
    {
        StatusText = Strings.Current.StatusGenerating;
        try
        {
            var mesh = await Task.Run(() =>
            {
                var combined = new Mesh { Name = "usmiechniety_chomik" };
                if (ShowWheel) combined.Merge(WheelGenerator.CreateWheel(WheelParams));
                if (ShowBase)
                {
                    var baseMesh = BaseGenerator.CreateBase(BaseParams, WheelParams);
                    combined.Merge(Primitives.Transform(baseMesh, 0, 0, -WheelParams.DiameterMm / 2f - 8f));
                }
                return combined;
            });

            CurrentMesh = mesh;
            MeshInfo = $"{mesh.Triangles.Count:N0} trójkątów / triangles";
            StatusText = $"✓ {MeshInfo}";
            Log.Information("Model generated: {Count} triangles", mesh.Triangles.Count);
        }
        catch (Exception ex)
        {
            StatusText = $"Błąd / Error: {ex.Message}";
            Log.Error(ex, "Model generation failed");
        }
    }

    [RelayCommand]
    public async Task GenerateTestSocketAsync()
    {
        StatusText = Strings.Current.StatusGenerating;
        CurrentMesh = await Task.Run(() => BaseGenerator.CreateTestSocket(BaseParams));
        MeshInfo = $"Test socket — {CurrentMesh?.Triangles.Count:N0} trójkątów";
        StatusText = $"✓ {MeshInfo}";
    }

    [RelayCommand]
    public void ApplySpeciesDefaults()
    {
        var species = SpeciesDatabase.Get(SelectedSpecies);
        if (species is null) return;
        var preset = ChomikPreset.FromSpecies(species);
        WheelParams = preset.Wheel;
        BaseParams = preset.Base;

        // Auto-suggest bearing and compute optimal bearing circle radius
        AutoApplyBearing(WheelParams.DiameterMm);
        Validate();
    }

    private void AutoApplyBearing(float wheelDiamMm)
    {
        var spec = DesignBearingSelector.Select(new DesignBearingQuery(wheelDiamMm));
        var match = BearingDatabase.All.FirstOrDefault(b => b.Code == spec.Id);
        if (match is not null)
            BaseParams.BearingCode = match.Code;

        var design = DesignBaseDesigner.Design(new DesignBaseQuery(wheelDiamMm, spec));
        BaseParams.BearingRadiusMm = (float)design.BearingCircleRadiusMm;
        BaseParams.BearingCount = design.BearingCount;
    }

    [RelayCommand]
    public void LoadStl()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Wczytaj STL / Load STL",
            Filter = "STL (*.stl)|*.stl|All (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true) return;
        try
        {
            CurrentMesh = StlImporter.Load(dlg.FileName);
            MeshInfo = $"{Path.GetFileName(dlg.FileName)} — {CurrentMesh.Triangles.Count:N0} trójkątów";
            StatusText = $"✓ {MeshInfo}";
        }
        catch (Exception ex)
        {
            StatusText = $"Błąd: {ex.Message}";
            Log.Error(ex, "Failed to load STL");
        }
    }

    [RelayCommand]
    public void ExportModel()
    {
        if (CurrentMesh is null)
        {
            StatusText = "Brak modelu / No model.";
            return;
        }

        var (ext, fmt) = SelectedFormatIndex switch
        {
            0 => (".stl", ExportFormat.StlBinary),
            1 => (".stl", ExportFormat.StlAscii),
            2 => (".obj", ExportFormat.Obj),
            3 => (".ply", ExportFormat.Ply),
            _ => (".stl", ExportFormat.StlBinary)
        };

        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName = CurrentMesh.Name,
            Filter = $"Model 3D (*{ext})|*{ext}|All (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            ModelExporter.Export(CurrentMesh, dlg.FileName, fmt);
            StatusText = Strings.Current.StatusExported + Path.GetFileName(dlg.FileName);
            Log.Information("Exported model to {Path}", dlg.FileName);
        }
        catch (Exception ex)
        {
            StatusText = $"Błąd eksportu: {ex.Message}";
            Log.Error(ex, "Model export failed");
        }
    }

    [RelayCommand]
    public void SavePreset()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Preset (*.json)|*.json",
            FileName = "chomik_preset"
        };

        if (dlg.ShowDialog() != true) return;
        var preset = new ChomikPreset
        {
            Species = SelectedSpecies,
            Wheel = WheelParams,
            Base = BaseParams,
            Printer = PrinterParams
        };
        preset.Save(dlg.FileName);
        StatusText = $"✓ Preset zapisany / saved: {Path.GetFileName(dlg.FileName)}";
    }

    [RelayCommand]
    public void LoadPreset()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Preset (*.json)|*.json"
        };
        if (dlg.ShowDialog() != true) return;

        var preset = ChomikPreset.Load(dlg.FileName);
        if (preset is null)
        {
            StatusText = "Nie można wczytać presetu / Could not load preset.";
            return;
        }

        SelectedSpecies = preset.Species;
        WheelParams = preset.Wheel;
        BaseParams = preset.Base;
        PrinterParams = preset.Printer;
        StatusText = $"✓ Preset wczytany / loaded: {preset.Name}";
    }

    public void SwitchLanguage()
    {
        var next = Strings.Current.Language == AppLanguage.Polish ? AppLanguage.English : AppLanguage.Polish;
        Strings.Current.SetLanguage(next);
        OnPropertyChanged(nameof(AllSpecies));
        OnPropertyChanged(nameof(PrinterWarning));
        StatusText = Strings.Current.StatusReady;
        Validate();
    }
}
