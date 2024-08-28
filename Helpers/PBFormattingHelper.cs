namespace ReportMigration.Helpers;

internal static class PBFormattingHelper
{
    public static double ConvertX(double value)
    {
        return Math.Ceiling((value * 1400 + 700) / 6144);
    }

    public static double ConvertY(double value)
    {
        return Math.Ceiling((value * 200 + 100) / 768);
    }

    public static string? ConvertElementType(string ctrlType)
    {
        return ctrlType switch
        {
            "header" => "ReportHeader",
            "footer" => "ReportFooter",
            "detail" => "Detail",
            "report" => "XRSubreport",
            "line" => "XRLine",
            "group" => ctrlType,
            "rectangle" => "XRShape",
            "text" or "column" or "compute"=> "XRLabel",
            _ => null,
        };
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
        return alignment switch
        {
            "0" => "MiddleLeft",
            "1" => "MiddleRight",
            "2" => "MiddleCenter",
            "3" => "MiddleJustify",
            _ => throw new Exception($"Unsupported alignment type: {alignment}"),
        };
    }

    public static List<string> GetParameters(string paramString)
    {
        var parameters = new List<string>();

        foreach (var param in paramString.Trim('(', ')').Split("),("))
        {
            var endIndex = param.IndexOf(',');
            if(endIndex >= 0)
            {
                parameters.Add(param[..endIndex].Trim());
            }
            else
            {
                parameters.Add(param.Trim());
            }
        }

        return parameters;
    }
}
