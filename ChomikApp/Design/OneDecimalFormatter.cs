using System.Globalization;
using ModernWpf.Controls;

namespace ChomikApp.Design;

public class OneDecimalFormatter : INumberBoxNumberFormatter
{
    public string FormatDouble(double value)
        => Math.Round(value, 1).ToString("F1", CultureInfo.CurrentCulture);

    public double? ParseDouble(string text)
    {
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out double v))
            return Math.Round(v, 1);
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
            return Math.Round(v, 1);
        return null;
    }
}
