using ReportMigration.Models;
using ReportMigration.Parser;
using ReportMigration.Helpers;

namespace ReportMigration.Converters;

internal class PblToRepxConverter(string inputDir, string outputPath)
{
    //private readonly List<ContainerModel> _structure;
    private int _ref = 1;
    private readonly StreamWriter _writer = new(outputPath);
    private readonly string _inputDir = inputDir;
    //private int height;
    //private int width;
    private int _tabulator = 0;
    private int _groupCount;
    private TableModel? _tableContainer;
    private Dictionary<string, int> _globalParams = [];

    public void GenerateRepxFile(string fileName)
    {
        PBReportParser parser = new(Path.Combine(_inputDir, fileName));
        var structure = parser.Parse();
        _groupCount = parser.groupCount;
        WriteSingleLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        var dataReportAttributes = structure[0]._attributes;
        WriteStartObject($"<XtraReportsLayoutSerializer SerializerVersion=\"24.1.4.0\" Ref=\"{_ref++}\" ControlType=\"DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v24.1, Version=24.1.4.0, Culture=neutral\" Name=\"XtraReport1\" VerticalContentSplitting=\"Smart\" Margins=\"{dataReportAttributes["print.margin.left"]}, {dataReportAttributes["print.margin.right"]}, {dataReportAttributes["print.margin.top"]}, {dataReportAttributes["print.margin.bottom"]}\" PaperKind=\"Custom\" PageWidth=\"2000\" PageHeight=\"1500\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-0\">");
        GenerateBody(structure, 0);

        WriteEndObject("</XtraReportsLayoutSerializer>");
        _writer.Flush();
    }

    public List<string> GenerateBody(List<ContainerModel> structure, int dataSourceRef)
    {
        _tableContainer = (TableModel)structure.Where(x => x.GetType() == typeof(TableModel)).ToList()[0];
        var parameters = GenerateDataSource(_tableContainer, dataSourceRef);
        WriteStartObject("<Bands>");
        int itemCounter = 1;
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"TopMarginBand\" Name=\"TopMargin\"/>");
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"BottomMarginBand\" Name=\"BottomMargin\"/>");
        foreach (var container in structure[1..])
        {
            switch (container._objectType)
            {
                case "group":
                    {
                        GenerateGroupElements(container, ref itemCounter);
                        break;
                    }
                case "table":
                    {
                        break;
                    }
                default:
                    {
                        GenerateElement(container, ref itemCounter);
                        break;
                    }
            }
        }

        WriteEndObject("</Bands>");

        return parameters;
    }

    private void GenerateSubReport(ObjectModel subreport, ref int itemCounter)
    {
        var objType = PBFormattingHelper.ConvertElementType(subreport._objectType);
        var attributes = subreport._attributes;
        var parameters = Parameters(attributes["nest_arguments"]);
        PBReportParser parser = new(Path.Combine(_inputDir, $"{attributes["dataobject"]}.p"));
        var structure = parser.Parse();
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\">");
        var dataWindowAttributes = structure[0]._attributes;
        var dataSourceRef = _ref++;
        WriteStartObject($"<ReportSource Ref=\"{_ref++}\" ControlType=\"ReportMigration.XtraReports.{attributes["dataobject"]}, ReportMigration, Version=1.0.0.0, Culture=neutral\" VerticalContentSplitting=\"Smart\" Margins=\"{dataWindowAttributes["print.margin.left"]}, {dataWindowAttributes["print.margin.right"]}, {dataWindowAttributes["print.margin.top"]}, {dataWindowAttributes["print.margin.bottom"]}\" PaperKind=\"Custom\" PageWidth=\"{X(attributes["width"])}\" PageHeight=\"{Y(attributes["height"])}\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-{dataSourceRef}\">");
        var paramsList = GenerateBody(structure, dataSourceRef);
        WriteEndObject("</ReportSource>");
        WriteStartObject("<ParameterBindings>");
        var subItemCounter = 0;
        foreach (var parameter in parameters)
        {
            if (_globalParams.TryGetValue(parameter, out var refNum))
            {
                var paramName = paramsList[subItemCounter++];
                WriteSingleLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" Parameter=\"#Ref-{refNum}\" />");
            }
            else
            {
                var paramName = paramsList[subItemCounter++];
                WriteSingleLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" DataMember=\"Query.{parameter}\" />");
            }
        }
        WriteEndObject("</ParameterBindings>");

        WriteEndObject($"</Item{itemCounter++}>");
    }

    public void GenerateElement(ContainerModel container, ref int itemCounter)
    {
        var objType = PBFormattingHelper.ConvertElementType(container._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = container._attributes;
        var elements = container._elements;
        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}Band\" Name=\"{objType}\" HeightF=\"{Y(attributes["height"])}\">");
        WriteStartObject("<Controls>");
        foreach (var element in elements)
        {
            if (objType == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateSubElement(element, ref subItemCounter);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    public void GenerateGroupElements(ContainerModel container, ref int itemCounter)
    {
        var objType = PBFormattingHelper.ConvertElementType(container._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = container._attributes;
        var elements = container._elements;

        if (!int.TryParse(attributes["level"], out var level))
        {
            throw new Exception($"Group element {attributes["name"]} is missing level attribute");
        }

        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();

        // Generate Group Header
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupHeaderBand\" Name=\"groupHeaderBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{Y(attributes["header.height"])}\">");
        var groupBy = GroupBy(attributes["by"]);
        WriteStartObject("<GroupFields>");
        foreach (var elem in groupBy)
        {
            WriteSingleLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" FieldName=\"{elem}\" />");
        }
        subItemCounter = 1;
        WriteEndObject("</GroupFields>");
        WriteStartObject("<Controls>");
        foreach (var element in elements.Where(x => x._attributes["band"].Contains("header")))
        {
            if (PBFormattingHelper.ConvertElementType(element._objectType) == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter);
            }
        }
        foreach (var element in backgroundShapes.Where(x => x._attributes["band"].Contains("header")))
        {
            GenerateSubElement(element, ref subItemCounter);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");

        // Generate Group Footer
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupFooterBand\" Name=\"groupFooterBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{Y(attributes["trailer.height"])}\">");
        WriteStartObject("<Controls>");
        subItemCounter = 1;
        backgroundShapes.Clear();
        foreach (var element in elements.Where(x => x._attributes["band"].Contains("trailer")))
        {
            if (PBFormattingHelper.ConvertElementType(element._objectType) == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter);
            }
        }
        foreach (var element in backgroundShapes.Where(x => x._attributes["band"].Contains("trailer")))
        {
            GenerateSubElement(element, ref subItemCounter);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    public void GenerateSubElement(ObjectModel element, ref int itemCounter)
    {
        var objType = PBFormattingHelper.ConvertElementType(element._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = element._attributes;

        switch (element._objectType)
        {
            case "report":
                {
                    GenerateSubReport(element, ref itemCounter);
                    break;
                }
            case "column":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextFormatString=\"{{0:{FixFormattingString(attributes["format"])}}}\" TextAlignment=\"{Alignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\">");
                    WriteStartObject("<ExpressionBindings>");
                    WriteSingleLine($"<Item1 Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"[{attributes["name"]}]\"/>");
                    WriteEndObject("</ExpressionBindings>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
            case "text":
                {
                    WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextAlignment=\"{Alignment(attributes["alignment"])}\" Multiline=\"true\" Text=\"{attributes["text"].Replace('&', '-')}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" />");
                    break;
                }
            case "compute":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextFormatString=\"{{0:{FixFormattingString(attributes["format"])}}}\" TextAlignment=\"{Alignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\">");
                    WriteStartObject("<ExpressionBindings>");
                    WriteSingleLine($"<Item1 Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"{attributes["expression"].Replace("IF", "Iif")}\"/>");
                    WriteEndObject("</ExpressionBindings>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
            case "line":
                {
                    var (x1, x2, y) = (attributes["x1"], attributes["x2"], attributes["y1"]);
                    if(!attributes.TryGetValue("background.gradient.color", out var color))
                    {
                        color = "";
                    }
                    WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(color)}\" SizeF=\"{X(x2)-X(x1)},10\" LocationFloat=\"{X(x1)},{Y(y)}\" />");
                    break;
                }
            case "rectangle":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(attributes["background.gradient.color"])}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\">");
                    WriteSingleLine($"<Shape Ref=\"{_ref++}\" ShapeName=\"Rectangle\"/>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
        }
    }

    private List<string> GenerateDataSource(TableModel table, int dataSourceRef)
    {
        var paramsList = new List<string>();
        //List<(string paramName, int refNum)> paramList = [];
        if (table._attributes.TryGetValue("arguments", out var parameters))
        {
            WriteStartObject("<Parameters>");
            var itemCounter = 1;
            foreach (var element in Parameters(parameters))
            {
                if (!_globalParams.ContainsKey(element))
                {
                    _globalParams.Add(element, _ref);
                }
                paramsList.Add(element);
                WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{element}\"/>");
            }
            WriteEndObject("</Parameters>");
        }
        WriteStartObject("<ComponentStorage>");
        WriteSingleLine($"<Item1 Ref=\"{dataSourceRef}\" ObjectType=\"DevExpress.DataAccess.Sql.SqlDataSource,DevExpress.DataAccess.v24.1\" Name=\"sqlDataSource1\" Base64=\"{DataSourceXmlGenerator.GenerateDataSourceXML(table)}\"/>");
        WriteEndObject("</ComponentStorage>");
        return paramsList;
    }

    private static double X(string value)
    {
        if (!double.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }

        return PBFormattingHelper.ConvertX(numValue);
    }

    private static double Y(string value)
    {
        if (!double.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }

        return PBFormattingHelper.ConvertY(numValue);
    }

    private static string Color(string value)
    {
        if(value == "")
        {
            return "Black";
        }
        if (!int.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }
        return PBFormattingHelper.ConvertColor(numValue);
    }

    private static string Alignment(string value)
    {
        return PBFormattingHelper.ConvertAlignment(value);
    }

    private static string[] GroupBy(string groupString)
    {
        return groupString.Trim('(', ')').Split(',');
    }

    private static List<string> Parameters(string paramString)
    {
        return PBFormattingHelper.GetParameters(paramString);
    }

    private static string Expression(string expression)
    {
        //return PBFormattingHelper.ParseExpression(expression);
        return "";
    }

    private static string FixFormattingString(string value)
    {
        if(value.ToLower() == "[general]")
        {
            return "";
        }
        return value.ToLower().Replace("mm", "MM");
    }

    private static string CheckBold(string value)
    {
        if (!int.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }
        if(numValue >= 700)
        {
            return ", style=Bold";
        }
        return "";
    }

    private void WriteStartObject(string str)
    {
        WriteSingleLine(str);
        _tabulator++;
    }

    private void WriteEndObject(string str)
    {
        _tabulator--;
        WriteSingleLine(str);
    }

    private void WriteSingleLine(string str)
    {
        for (var i = 0; i < _tabulator; i++)
        {
            _writer.Write("\t");
        }
        _writer.WriteLine(str);
    }
}
