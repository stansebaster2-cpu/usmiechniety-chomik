using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ChomikApp.Localization;

namespace ChomikApp.Dialogs;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        Title = Strings.Current.AboutTitle;
    }

    void OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("http"))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    void GithubLink_Click(object sender, MouseButtonEventArgs e)
        => OpenUrl(Strings.Current.AboutGithubUrl);

    void CoffeeLink_Click(object sender, MouseButtonEventArgs e)
        => OpenUrl(Strings.Current.AboutBuyMeACoffeeUrl);

    void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
