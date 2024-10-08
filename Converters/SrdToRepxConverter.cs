﻿using ReportMigration.Models;
using ReportMigration.Parser;
using static ReportMigration.Helpers.DataSourceXmlGenerator;
using static ReportMigration.Helpers.PBFormattingHelper;
using ReportMigration.Helpers;
using System.Diagnostics; 

namespace ReportMigration.Converters;

internal class SrdToRepxConverter(string inputDir, string outputDir)
{
    private int _ref = 1;
    private StreamWriter? _writer;
    private readonly PBExpressionParser _parser = new();
    private readonly string _inputDir = inputDir;
    private readonly string _outputDir = outputDir;
    private int _tabulator = 0;
    private int _groupCount;
    private double _globalHeight;
    private double _globalWidth;
    private readonly Dictionary<string, int> _globalParams = [];
    private readonly List<(string name, string expression)> _currentComputes = [];

    public void GenerateRepxFile(string fileName)
    {
        Console.Write($"Converting {fileName} to .repx");
        var inputPath = Path.Combine(_inputDir, fileName);
        var extensionIndex = fileName.LastIndexOf('.');
        var outputPath = Path.Combine(_outputDir, $"{fileName[..extensionIndex]}.repx");
        try
        {
            PBReportParser parser = new(inputPath);
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            else { 
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);            
            }

            _writer = new(outputPath);

            var structure = parser.Parse();
            _groupCount = parser.GroupCount;
            _globalHeight = parser.ReportHeight;
            _globalWidth = parser.ReportWidth;

            WriteSingleLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            var dataWindowIndex = structure.FindIndex(x => x._objectType == "datawindow");
            var dataWindowAttributes = structure[dataWindowIndex]._attributes;
            WriteStartObject($"<XtraReportsLayoutSerializer SerializerVersion=\"24.1.4.0\" Ref=\"{_ref++}\" ControlType=\"DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v24.1, Version=24.1.4.0, Culture=neutral\" Name=\"XtraReport1\" VerticalContentSplitting=\"Smart\" Margins=\"{X(dataWindowAttributes["print.margin.left"])}, {X(dataWindowAttributes["print.margin.right"])}, {Y(dataWindowAttributes["print.margin.top"])}, {Y(dataWindowAttributes["print.margin.bottom"])}\" PaperKind=\"Custom\" PageWidth=\"{parser.ReportWidth + X(dataWindowAttributes["print.margin.left"]) + X(dataWindowAttributes["print.margin.right"])}\" PageHeight=\"{parser.ReportHeight + Y(dataWindowAttributes["print.margin.top"]) + Y(dataWindowAttributes["print.margin.bottom"]) + 200}\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-0\">");

            var tableContainer = (TableModel)structure.Where(x => x.GetType() == typeof(TableModel)).ToList()[0];


            if (tableContainer._attributes.ContainsKey("arguments"))
                GenerateParameters(PBFormattingHelper.GetParameters(tableContainer._attributes["arguments"]));
            GenerateDataSource(tableContainer, 0);
            GenerateBody(structure, dataWindowIndex);

            WriteEndObject("</XtraReportsLayoutSerializer>");
            _writer.Flush();
            _writer.Dispose();

            _ref = 1;
            _tabulator = 0;
            _globalParams.Clear();
            _currentComputes.Clear();
            Console.Write(" - Done\n");
        }
        catch (Exception e)
        { 
            Console.WriteLine($" - Failed\n\t{e.Message}");
            if (File.Exists(outputPath) && _writer != null)
            { 
                _writer.Flush();
                _writer.Dispose();
                File.Delete(outputPath); 
            }
        }
    }

    public void GenerateBody(List<ContainerModel> structure, int dataWindowIndex)
    {
        var dataReportAttributes = structure[dataWindowIndex]._attributes;
        WriteStartObject("<Bands>");
        int itemCounter = 1;
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"TopMarginBand\" Name=\"TopMargin\" HeightF=\"{Y(dataReportAttributes["print.margin.top"])}\"/>");
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"BottomMarginBand\" Name=\"BottomMargin\" HeightF=\"{Y(dataReportAttributes["print.margin.bottom"])}\"/>");
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

        WriteStartObject("<CalculatedFields>");
        itemCounter = 1;
        foreach (var (name, expression) in _currentComputes)
        {
            WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{name}\" Expression=\"{expression}\" DataMember=\"Query\" />");
        }
        WriteEndObject("</CalculatedFields>");
        _currentComputes.Clear();
    }

    private void GenerateSubReport(ObjectModel subreport, ref int itemCounter)
    {
        var objType = ConvertElementType(subreport._objectType);
        var attributes = subreport._attributes;
        var parameterArguments = GetParameters(attributes["nest_arguments"]);

        var tmpHeight = _globalHeight;
        var tmpWidth = _globalWidth;
        var tmpGroupCount = _groupCount;

        PBReportParser parser = new(Path.Combine(_inputDir, $"{attributes["dataobject"]}.p"));
        var structure = parser.Parse();

        _globalHeight = parser.ReportHeight;
        _globalWidth = parser.ReportWidth;
        _groupCount = parser.GroupCount;

        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["dataobject"]}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\">");
        var dataWindowIndex = structure.FindIndex(x => x._objectType == "datawindow");
        var dataWindowAttributes = structure[dataWindowIndex]._attributes;
        var dataSourceRef = _ref++;

        WriteStartObject($"<ReportSource Ref=\"{_ref++}\" Name=\"{attributes["dataobject"]}\" ControlType=\"ReportMigration.XtraReports.{attributes["dataobject"]}, ReportMigration, Version=1.0.0.0, Culture=neutral\" VerticalContentSplitting=\"Smart\" Margins=\"{X(dataWindowAttributes["print.margin.left"])}, {X(dataWindowAttributes["print.margin.right"])}, {Y(dataWindowAttributes["print.margin.top"])}, {Y(dataWindowAttributes["print.margin.bottom"])}\" PaperKind=\"Custom\" PageWidth=\"{parser.ReportWidth}\" PageHeight=\"{parser.ReportHeight}\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-{dataSourceRef}\">");

        var tableContainer = (TableModel)structure.Where(x => x.GetType() == typeof(TableModel)).ToList()[0];
        var argList = GetParameters(tableContainer._attributes["arguments"]);

        GenerateParameters(argList);
        GenerateDataSource(tableContainer, dataSourceRef);
        GenerateBody(structure, dataWindowIndex);

        WriteEndObject("</ReportSource>");

        WriteStartObject("<ParameterBindings>");
        var subItemCounter = 0;
        foreach (var arg in parameterArguments)
        {
            var paramName = argList[subItemCounter++];
            if (_globalParams.TryGetValue(arg, out var refNum))
            {
                WriteSingleLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" Parameter=\"#Ref-{refNum}\" />");
            }
            else
            {
                WriteSingleLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" DataMember=\"Query.{arg}\" />");
            }
        }
        WriteEndObject("</ParameterBindings>");

        WriteEndObject($"</Item{itemCounter++}>");

        _globalHeight = tmpHeight;
        _globalWidth = tmpWidth;
        _groupCount = tmpGroupCount;
    }

    private void GenerateParameters(List<string> arguments)
    {
        WriteStartObject("<Parameters>");
        var itemCounter = 1;
        foreach (var element in arguments)
        {
            _globalParams.TryAdd(element, _ref);
            WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{element}\"/>");
        }
        WriteEndObject("</Parameters>");
    }

    public void GenerateElement(ContainerModel container, ref int itemCounter)
    {
        var objType = ConvertElementType(container._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = container._attributes;
        var elements = container._elements;
        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();
        var height = Y(attributes["height"]);
        var summaryLevel = container._objectType == "summary" ? $" Level=\"{_groupCount}\"" : "";

        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}Band\" Name=\"{container._objectType}\" HeightF=\"{height}\"{SetVisible(height)}{summaryLevel}>");
        WriteStartObject("<Controls>");
        foreach (var element in elements)
        {
            objType = ConvertElementType(element._objectType);
            if (objType == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateSubElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    private static string SetVisible(double height)
    {
        if (height > 0)
        {
            return "";
        }
        return " Visible=\"False\"";
    }

    public void GenerateGroupElements(ContainerModel container, ref int itemCounter)
    {
        var attributes = container._attributes;
        var elements = container._elements;

        if (!int.TryParse(attributes["level"], out var level))
        {
            throw new Exception($"Group element {attributes["name"]} is missing level attribute");
        }

        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();

        // Generate Group Header
        var height = Y(attributes["header.height"]);
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupHeaderBand\" Name=\"groupHeaderBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{height}\"{SetVisible(height)}>");
        var groupBy = attributes["by"].Trim('(', ')').Split(',');
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
            var objType = ConvertElementType(element._objectType);
            if (objType == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateSubElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");

        // Generate Group Footer
        height = Y(attributes["trailer.height"]);
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupFooterBand\" Name=\"groupFooterBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{height}\"{SetVisible(height)}>");
        WriteStartObject("<Controls>");
        subItemCounter = 1;
        backgroundShapes.Clear();
        foreach (var element in elements.Where(x => x._attributes["band"].Contains("trailer")))
        {
            if (ConvertElementType(element._objectType) == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateSubElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateSubElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    public void GenerateSubElement(ObjectModel element, ref int itemCounter, ContainerModel container)
    {
        var objType = ConvertElementType(element._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = element._attributes;
        double x;
        double y;
        if (!attributes.TryGetValue("x", out var xstr))
        {
            x = X(attributes["x1"]);
            y = Y(attributes["y1"]);
        }
        else
        {
            x = X(xstr);
            y = Y(attributes["y"]);
        }

        double containerHeight;
        var band = attributes["band"];

        if (band.Contains("header."))
        {
            containerHeight = Y(container._attributes["header.height"]);
        }
        else if (band.Contains("trailer."))
        {
            containerHeight = Y(container._attributes["trailer.height"]);
        }
        else
        {
            containerHeight = Y(container._attributes["height"]);
        }

        bool visibility;
        if (attributes["visible"] == "0")
        {
            visibility = false;
        }
        else
        {
            visibility = x < _globalWidth && y < containerHeight;
        }

        switch (element._objectType)
        {
            case "report":
                {
                    GenerateSubReport(element, ref itemCounter);
                    break;
                }
            case "column":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextFormatString=\"{FixFormattingString(attributes["format"])}\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\">");
                    WriteStartObject("<ExpressionBindings>");
                    WriteSingleLine($"<Item1 Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"[{attributes["name"]}]\"/>");
                    WriteEndObject("</ExpressionBindings>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
            case "text":
                {
                    WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\" Multiline=\"true\" Text=\"{attributes["text"].Replace('&', '-')}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\"/>");
                    break;
                }
            case "compute":
                {
                    var name = attributes["name"];
                    if(attributes["expression"] == "page()")
                    {
                        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"XRPageInfo\" Name=\"{name}\" PageInfo=\"Number\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" />");
                    }
                    else
                    {
                        var expression = Expression(attributes["expression"]);
                        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{name}_field\" TextFormatString=\"{FixFormattingString(attributes["format"])}\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\">");
                        WriteStartObject("<ExpressionBindings>");
                        WriteSingleLine($"<Item1 Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"[{name}]\"/>");
                        WriteEndObject("</ExpressionBindings>");
                        WriteEndObject($"</Item{itemCounter++}>");
                        _currentComputes.Add((name, expression));
                    }
                    break;
                }
            case "line":
                {
                    var (x2, y2) = (X(attributes["x2"]), Y(attributes["y2"]));
                    if (!attributes.TryGetValue("background.gradient.color", out var color))
                    {
                        color = "";
                    }
                    double length;
                    double height;

                    if (x2 > _globalWidth)
                    {
                        length = _globalWidth - x;
                    }
                    else
                    {
                        length = x2 - x;
                    }
                    if (y2 > containerHeight)
                    {
                        height = containerHeight - y;
                    }
                    else
                    {
                        height = y2 - y;
                    }
                    var direction = length == 0 ? "Vertical" : "Horizontal";
                    WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(color)}\" SizeF=\"{length},{height}\" LocationFloat=\"{x},{y}\"  LineDirection=\"{direction}\" Visible=\"{visibility}\"/>");
                    break;
                }
            case "rectangle":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(attributes["background.gradient.color"])}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" Visible=\"{visibility}\">");
                    WriteSingleLine($"<Shape Ref=\"{_ref++}\" ShapeName=\"Rectangle\"/>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
        }
    }

    private void GenerateDataSource(TableModel table, int dataSourceRef)
    {
        WriteStartObject("<ComponentStorage>");
        WriteSingleLine($"<Item1 Ref=\"{dataSourceRef}\" ObjectType=\"DevExpress.DataAccess.Sql.SqlDataSource,DevExpress.DataAccess.v24.1\" Name=\"sqlDataSource1\" Base64=\"{GenerateDataSourceXML(table)}\"/>");
        WriteEndObject("</ComponentStorage>");
    }

    private string Expression(string expression)
    {
        var parsedExpression = _parser.Parse(expression);
        return parsedExpression;
    }

    private static string FixFormattingString(string value)
    {
        if (value.Equals("[general]", StringComparison.CurrentCultureIgnoreCase))
        {
            return "";
        }

        return $"{{0:{value.ToLower().Replace("mm", "MM")}}}";
    }

    private static string CheckBold(string value)
    {
        if (!int.TryParse(value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as int");
        }
        if (numValue >= 700)
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
            _writer!.Write("\t");
        }
        _writer!.WriteLine(str);
    }
}
