namespace PBReportConverter.Helpers;

internal static class PBFormattingHelper
{
    public static double X(string value)
    {
        if (!double.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }
        return Math.Ceiling((numValue * 1400 - 700) / 6144);
    }

    public static double Y(string value)
    {
        if (!double.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }
        return Math.Ceiling((numValue * 200 - 100) / 768);
    }

    public static string? ConvertElementType(string ctrlType)
    {
        return ctrlType switch
        {
            "header" => "ReportHeader",
            "footer" => "ReportFooter",
            "detail" => "Detail",
            "summary" => "GroupHeader",
            "report" => "XRSubreport",
            "line" => "XRLine",
            "group" => ctrlType,
            "rectangle" => "XRShape",
            "text" or "column" or "compute"=> "XRLabel",
            _ => null,
        };
    }

    public static string Color(string colorCode)
    {
        if (colorCode == "")
        {
            return "Black";
        }
        if (!int.TryParse(colorCode, out var numColorCode))
        {
            throw new Exception($"Couldn't parse value: {colorCode} as int");
        }
        var red = numColorCode % 256;
        numColorCode /= 256;
        var green = numColorCode % 256;
        var blue = numColorCode / 256;
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
