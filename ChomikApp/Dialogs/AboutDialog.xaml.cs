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
        var assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        var photoPath = new[] { "zosia.jpg", "zosia.jpeg", "zosia.png" }
            .Select(f => Path.Combine(assetsDir, f))
            .FirstOrDefault(File.Exists);
        if (photoPath is null)
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

    private bool _photoDragging;
    private System.Windows.Point _photoDragStart;

    void PhotoBorder_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (ZosiaPhoto.Visibility != Visibility.Visible) return;
        double factor = e.Delta > 0 ? 1.15 : 1 / 1.15;
        double newScale = Math.Clamp(PhotoScale.ScaleX * factor, 1.0, 8.0);
        PhotoScale.ScaleX = newScale;
        PhotoScale.ScaleY = newScale;
        if (newScale <= 1.0) { PhotoTranslate.X = 0; PhotoTranslate.Y = 0; }
        e.Handled = true;
    }

    void PhotoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (PhotoScale.ScaleX <= 1.0) return;
        _photoDragging = true;
        _photoDragStart = e.GetPosition((UIElement)sender);
        ((UIElement)sender).CaptureMouse();
    }

    void PhotoBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _photoDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    void PhotoBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_photoDragging) return;
        var pos = e.GetPosition((UIElement)sender);
        PhotoTranslate.X += pos.X - _photoDragStart.X;
        PhotoTranslate.Y += pos.Y - _photoDragStart.Y;
        _photoDragStart = pos;
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
