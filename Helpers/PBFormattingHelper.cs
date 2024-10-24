using PBReportConverter.Parser;

namespace PBReportConverter.Helpers;

internal static class PBFormattingHelper
{
    public static int uom;
    private static readonly PBExpressionParser _parser = new();
    public static double X(string value)
    {
        if (!double.TryParse(value.Split("~t")[0], out var numValue))
        {
            throw new ArgumentException($"Couldn't parse value: {value} as int");
        }
        return uom switch
        {
            0 => Math.Ceiling((numValue * 1400 - 700) / 6144),
            1 => numValue,
            2 => Math.Ceiling(numValue /10),
            3 => Math.Ceiling(numValue / 10 * 2.54),
            _ => throw new NotImplementedException($"Unit not implemented: {uom}")
        };
    }

    public static double Y(string value)
    {
        if (!double.TryParse(value.Split("~t")[0], out var numValue))
        {
            throw new ArgumentException($"Couldn't parse value: {value} as int");
        }
        return uom switch
        {
            0 => Math.Ceiling((numValue * 200 - 100) / 768),
            1 => numValue,
            2 => Math.Ceiling(numValue / 10),
            3 => Math.Ceiling(numValue / 10 * 2.54),
            _ => throw new NotImplementedException($"Unit not implemented: {uom}")
        };
    }

    public static (string printEvent, string expr) CheckForExpressionString(string value)
    {
        var splitString = value.Split("~t");

        if(splitString.Length == 1)
        {
            return ("BeforePrint", string.Empty);
        }

        return Expression(splitString[1]);
    }

    public static (string printEvent, string expr) Expression(string expression)
    {
        return _parser.Parse(expression);
    }

    public static string MapAttr(string attr)
    {
        return attr switch
        {
            "visible" => "Visible",
            "height" => "HeightF",
            "width" => "WidthF",
            _ => throw new NotImplementedException($"Unmapped attribute: {attr}")
        };
    }

    public static string FixFormattingString(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("[general]", StringComparison.CurrentCultureIgnoreCase))
        {
            return "";
        }

        var formatStr = value.Split("~t")[0];

        return $"{{0:{formatStr.ToLower().Replace("mm", "MM")}}}";
    }

    public static string? ConvertElementType(string ctrlType)
    {
        return ctrlType switch
        {
            "header" => "ReportHeader",
            "footer" => "ReportFooter",
            "detail" => "Detail",
            "summary" => "GroupFooter",
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
