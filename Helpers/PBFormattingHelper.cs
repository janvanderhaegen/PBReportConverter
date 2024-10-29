using PBReportConverter.Parser;

namespace PBReportConverter.Helpers;

/// <summary>
/// Static helper class containing various methods for format conversion between PowerBuilder and DevExpress
/// </summary>
internal static class PBFormattingHelper
{
    // Unit of measure provided by the converter.
    // The possible units are:
    // 0 - PowerBuilder Unit
    // 1 - Display pixels
    // 2 - 1/1000 of a logical inch
    // 3 - 1/1000 of a logical centimeter
    public static int uom;

    private static readonly PBExpressionParser _parser = new();

    // Converts the provided X-coordinate number in string form into the equivalent number in hundredths of an inch, which is the default unit of measure for DevExpress.
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

    // Converts the provided Y-coordinate number in string form into the equivalent number in hundredths of an inch, which is the default unit of measure for DevExpress.
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

    // Checks if value has expression string and returns it in the DevExpress format if so.
    public static (string printEvent, string expr) CheckForExpressionString(string value)
    {
        var splitString = value.Split("~t");

        if(splitString.Length == 1)
        {
            return ("BeforePrint", string.Empty);
        }

        return Expression(splitString[1]);
    }

    // Parses and converts the given PowerBuilder expression into its DevExpress equivalent
    public static (string printEvent, string expr) Expression(string expression)
    {
        return _parser.Parse(expression);
    }

    // Maps PowerBuilder attribute into its DevExpress equivalent
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

    // Returns DevExpress equivalent of the text format string
    public static string FixFormattingString(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("[general]", StringComparison.CurrentCultureIgnoreCase))
        {
            return "";
        }

        if(value.Equals("[shortdate]", StringComparison.CurrentCultureIgnoreCase))
        {
            return "dd-MM-yyyy";
        }

        var formatStr = value.Split("~t")[0];
        Span<char> newFormatStr = stackalloc char[formatStr.Length];
        int pos = 0;
        foreach(char c in formatStr)
        {
            // TODO find way to discern between ms representing months and ms representing minutes
            // Until then, double-check any datetime formatting strings in the .repx files, as months should be represented with M and minutes with m.
            if(c == 'm')
            {
                newFormatStr[pos++] = 'M';
            }
            else
            {
                newFormatStr[pos++] = Char.ToLower(c);
            }
        }

        return $"{{0:{newFormatStr[..pos].ToString()}}}";
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

    // Converts encoded color code number into an RGB representation used in DevExpress.
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

    // Fetches text alignment
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

    // Gets list of subreport input parameters
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
