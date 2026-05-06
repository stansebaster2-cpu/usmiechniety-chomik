using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using ChomikApp.Dialogs;
using ChomikApp.Localization;
using ChomikApp.ViewModels;
using System.Diagnostics;

namespace ChomikApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private Direct3D11RenderControl? _render;
    private bool _speciesChanging;
    private bool _initialized;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainViewModel)DataContext!;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _render = new Direct3D11RenderControl();
        RenderHost.Child = _render;
        SpeciesCombo.SelectionChanged += SpeciesCombo_SelectionChanged;

        _viewModel.PropertyChanged += (_, ev) =>
        {
            if (ev.PropertyName == nameof(_viewModel.CurrentMesh) && _viewModel.CurrentMesh is not null)
            {
                _render?.LoadMesh(_viewModel.CurrentMesh);
                _render?.FitCamera();
            }
        };

        Title = Strings.Current.AppTitle;
        Strings.Current.LanguageChanged += () => Title = Strings.Current.AppTitle;

        _initialized = true;
    }

    private void SpeciesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized || !IsVisible || _speciesChanging) return;
        _speciesChanging = true;

        var dlg = new SpeciesPickerDialog();

        if (dlg.ShowDialog() == true && dlg.SelectedSpecies is not null)
        {
            _viewModel.SelectedSpecies = dlg.SelectedSpecies.Id;
            if (dlg.ApplyDefaults)
                _viewModel.ApplySpeciesDefaultsCommand.Execute(null);
        }

        _speciesChanging = false;
    }

    private void BtnLang_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SwitchLanguage();
        Title = Strings.Current.AppTitle;
    }

    private void BtnAbout_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AboutDialog { Owner = this };
        dlg.ShowDialog();
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.FocusedElement is UIElement focused)
        {
            focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _render?.Dispose();
        base.OnClosed(e);
    }
}
