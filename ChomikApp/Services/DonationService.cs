using System.Diagnostics;

namespace ChomikApp.Services;

public static class DonationService
{
    public const string DefaultUrl = "https://buymeacoffee.com/";

    public static void OpenDonationPage(string? customUrl = null)
    {
        var url = string.IsNullOrWhiteSpace(customUrl) ? DefaultUrl : customUrl;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
