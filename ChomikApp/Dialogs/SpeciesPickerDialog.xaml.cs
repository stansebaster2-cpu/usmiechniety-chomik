using System.Windows;
using System.Windows.Controls;
using ChomikApp.Localization;
using ChomikEngine.Data;

namespace ChomikApp.Dialogs;

public partial class SpeciesPickerDialog : Window
{
    public HamsterSpecies? SelectedSpecies { get; private set; }
    public bool ApplyDefaults { get; private set; }

    public SpeciesPickerDialog()
    {
        InitializeComponent();
        Title = Strings.Current.SpeciesDialogTitle;
        SpeciesList.ItemsSource = SpeciesDatabase.All;
        SpeciesList.SelectedIndex = 0;
    }

    void SpeciesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SpeciesList.SelectedItem is HamsterSpecies s)
        {
            HealthNoteText.Text = s.CurrentHealthNote;
            HealthNotePanel.Visibility = Visibility.Visible;
        }
    }

    void BtnApply_Click(object sender, RoutedEventArgs e)
    {
        SelectedSpecies = SpeciesList.SelectedItem as HamsterSpecies;
        ApplyDefaults = true;
        DialogResult = true;
    }

    void BtnCustomize_Click(object sender, RoutedEventArgs e)
    {
        SelectedSpecies = SpeciesList.SelectedItem as HamsterSpecies;
        ApplyDefaults = false;
        DialogResult = true;
    }
}
