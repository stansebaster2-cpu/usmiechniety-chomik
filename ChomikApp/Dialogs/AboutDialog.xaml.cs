using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ChomikApp.Localization;

namespace ChomikApp.Dialogs;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        Title = Strings.Current.AboutTitle;
        TryLoadZosiaPhoto();
    }

    void TryLoadZosiaPhoto()
    {
        var photoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "zosia.jpg");
        if (!File.Exists(photoPath))
            photoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "zosia.png");
        if (!File.Exists(photoPath))
            return;

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(photoPath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();

            ZosiaPhoto.Source = bmp;
            ZosiaPhoto.Visibility = Visibility.Visible;
            PhotoPlaceholder.Visibility = Visibility.Collapsed;
            PhotoCaption.Visibility = Visibility.Visible;
        }
        catch { }
    }

    void OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("http"))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    void GithubLink_Click(object sender, MouseButtonEventArgs e)
        => OpenUrl(Strings.Current.AboutGithubUrl);

    async void BtnCopyBlik_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText("505899450");
        var original = BtnCopyBlik.Content;
        BtnCopyBlik.Content = Strings.Current.AboutBlikCopied;
        BtnCopyBlik.IsEnabled = false;
        await System.Threading.Tasks.Task.Delay(2000);
        BtnCopyBlik.Content = original;
        BtnCopyBlik.IsEnabled = true;
    }

    void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
