using DevExpress.XtraGauges.Core.Model;

namespace ReportMigration.Helpers;

internal static class PBFormattingHelper
{
    public static int ConvertX(int value)
    {
        return (value * 1400 + 700) / 6144;
    }

    public static int ConvertY(int value)
    {
        return (value * 200 + 100) / 768;
    }

    public static string? ConvertElementType(string ctrlType)
    {
        switch (ctrlType)
        {
            case "header": return "ReportHeader";
            case "footer": return "ReportFooter";
            case "detail": return "Detail";
            case "group": return ctrlType;
            case "rectangle": return "XRShape";
            case "text":
            case "column": return "XRLabel";
            default: return null;
        }
    }

    public static string ConvertColor(int colorCode)
    {
        var red = colorCode % 256;
        colorCode /= 256;
        var green = colorCode % 256;
        var blue = colorCode / 256;
        return $"255,{red},{green},{blue}";
    }

    public static string ConvertAlignment(string alignment)
    {
        switch (alignment)
        {
            case "0": return "MiddleLeft";
            case "1": return "MiddleRight";
            case "2": return "MiddleCenter";
            case "3": return "MiddleJustify";
            default: throw new Exception($"Unsupported alignment type: {alignment}");
        }
    }
}
