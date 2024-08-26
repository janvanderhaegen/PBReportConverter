using ReportMigration.Models;
using ReportMigration.Parser;
using ReportMigration.Helpers;
using System.ComponentModel;
using DevExpress.XtraExport.Xls;
using DevExpress.PivotGrid.Criteria.Validation;

namespace ReportMigration.Converters;

internal class PblToRepxConverter
{
    private List<ContainerModel> _structure;
    private int _ref = 1;
    private readonly StreamWriter _writer;
    //private int height;
    //private int width;
    private int _tabulator = 0;
    private int _groupCount;
    private ContainerModel? _tableContainer;

    public PblToRepxConverter(string inputPath, string outputPath)
    {
        PBReportParser parser = new PBReportParser(inputPath);
        parser.Parse();
        _structure = parser.GetStructure();
        _groupCount = parser.groupCount;
        _writer = new StreamWriter(outputPath);
        //height = parser.reportHeight;
        //width = parser.reportWidth;
    }

    public void GenerateRepxFile()
    {
        WriteSingleLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        var dataReportAttributes = _structure[0]._attributes;
        WriteStartObject($"<XtraReportsLayoutSerializer SerializerVersion=\"24.1.4.0\" Ref=\"{_ref++}\" ControlType=\"DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v24.1, Version=24.1.4.0, Culture=neutral\" Name=\"XtraReport2\" VerticalContentSplitting=\"Smart\" Margins=\"{dataReportAttributes["print.margin.left"]}, {dataReportAttributes["print.margin.right"]}, {dataReportAttributes["print.margin.top"]}, {dataReportAttributes["print.margin.bottom"]}\" PaperKind=\"Custom\" PageWidth=\"1100\" PageHeight=\"1100\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-0\">");
        WriteStartObject("<Bands>");
        int itemCounter = 1;
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"TopMarginBand\" Name=\"TopMargin\"/>");
        WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"BottomMarginBand\" Name=\"BottomMargin\"/>");
        foreach(var container in _structure[1..])
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
                        _tableContainer = container;
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

        if (_tableContainer != null)
        {
            GenerateDataSource(_tableContainer);
        }

        WriteEndObject("</XtraReportsLayoutSerializer>");
        _writer.Flush();
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
            if (PBFormattingHelper.ConvertElementType(element._objectType) == "XRShape")
            {
                backgroundShapes.Add(element);
            }else
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

        if(!Int32.TryParse((string)attributes["level"], out var level))
        {
            throw new Exception($"Group element {attributes["name"]} is missing level attribute");
        }

        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();

        // Generate Group Header
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupHeaderBand\" Name=\"groupHeaderBand{attributes["level"]}\" Level=\"{_groupCount-level}\" HeightF=\"{Y(attributes["header.height"])}\">");
        var groupBy = (List<object>)attributes["by"];
        WriteStartObject("<GroupFields>");
        foreach (var elem in groupBy)
        {
            WriteSingleLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" FieldName=\"{elem}\" />");
        }
        subItemCounter = 1;
        WriteEndObject("</GroupFields>");
        WriteStartObject("<Controls>");
        foreach (var element in elements.Where(x => (x._attributes["band"] as string)!.Contains("header")))
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
        foreach (var element in backgroundShapes.Where(x => (x._attributes["band"] as string)!.Contains("header")))
        {
            GenerateSubElement(element, ref subItemCounter);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");

        // Generate Group Footer
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupFooterBand\" Name=\"groupFooterBand{attributes["level"]}\" Level=\"{_groupCount-level}\" HeightF=\"{Y(attributes["trailer.height"])}\">");
        WriteStartObject("<Controls>");
        subItemCounter = 1;
        backgroundShapes.Clear();
        foreach (var element in elements.Where(x => (x._attributes["band"] as string)!.Contains("trailer")))
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
        foreach (var element in backgroundShapes.Where(x => (x._attributes["band"] as string)!.Contains("trailer")))
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
            case "column":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextFormatString=\"{{0:{FixFormattingString(attributes["format"])}}}\" TextAlignment=\"{Alignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {((string)attributes["font.height"])[1..]}pt\">");
                    WriteStartObject("<ExpressionBindings>");
                    WriteSingleLine($"<Item1 Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"[{attributes["name"]}]\"/>");
                    WriteEndObject("</ExpressionBindings>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
            case "text":
                {
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" TextAlignment=\"{Alignment(attributes["alignment"])}\" Multiline=\"true\" Text=\"{attributes["text"]}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {((string)attributes["font.height"])[1..]}pt\">");
                    WriteEndObject($"</Item{itemCounter++}>");
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

    private void GenerateDataSource(ContainerModel table)
    {
        if (table._attributes.TryGetValue("arguments", out var parameters))
        {
            WriteStartObject("<Parameters>");
            var itemCounter = 1;
            foreach(var element in (List<object>)parameters)
            {
                WriteSingleLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{((List<object>)element)[0]}\"/>");
            }
            WriteEndObject("</Parameters>");
        }
        WriteStartObject("<ComponentStorage>");
        WriteSingleLine("<Item1 Ref=\"0\" ObjectType=\"DevExpress.DataAccess.Sql.SqlDataSource,DevExpress.DataAccess.v24.1\" Name=\"sqlDataSource1\" Base64=\"PFNxbERhdGFTb3VyY2UgTmFtZT0ic3FsRGF0YVNvdXJjZTEiPjxDb25uZWN0aW9uIE5hbWU9IlRlc3REQkNvbm5lY3Rpb24iIEZyb21BcHBDb25maWc9InRydWUiIC8+PFF1ZXJ5IFR5cGU9IlN0b3JlZFByb2NRdWVyeSIgTmFtZT0iaW52b2ljZV9hbmFfcmVwb3J0X3NwIj48UGFyYW1ldGVyIE5hbWU9IkBpbnZfbmJyIiBUeXBlPSJTeXN0ZW0uU3RyaW5nIj5NMTMwN1MwMDA2PC9QYXJhbWV0ZXI+PFBhcmFtZXRlciBOYW1lPSJAaW52X3JldiIgVHlwZT0iU3lzdGVtLkNoYXIiPjAwMjA8L1BhcmFtZXRlcj48UHJvY05hbWU+aW52b2ljZV9hbmFfcmVwb3J0X3NwPC9Qcm9jTmFtZT48L1F1ZXJ5PjxRdWVyeSBUeXBlPSJDdXN0b21TcWxRdWVyeSIgTmFtZT0iUXVlcnkiPjxTcWw+ZXhlY3V0ZSBpbnZvaWNlX2FuYV9yZXBvcnRfc3AgQGludl9uYnIgPSAnTTEzMDdTMDAwNicsQGludl9yZXYgPSAnICc8L1NxbD48L1F1ZXJ5PjxSZXN1bHRTY2hlbWE+PERhdGFTZXQgTmFtZT0ic3FsRGF0YVNvdXJjZTEiPjxWaWV3IE5hbWU9Imludm9pY2VfYW5hX3JlcG9ydF9zcCI+PEZpZWxkIE5hbWU9Imludm9pY2VfbmJyIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9Imludm9pY2VfcmV2IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9IkNvbHVtbjMiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icHJldmlvdXNfYW1vdW50IiBUeXBlPSJEb3VibGUiIC8+PEZpZWxkIE5hbWU9InByZXZpb3VzX2Ftb3VudF90eHQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0idG90YWxfYW1vdW50IiBUeXBlPSJEb3VibGUiIC8+PEZpZWxkIE5hbWU9ImNvbnRyYWN0X25iciIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXllcl9iYV9uYnIiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icG1udF90ZXJtc19kYXRlIiBUeXBlPSJEYXRlVGltZSIgLz48RmllbGQgTmFtZT0icG1udF90ZXJtc190eHQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iaW52b2ljZV9kYXRlIiBUeXBlPSJEYXRlVGltZSIgLz48RmllbGQgTmFtZT0iaW52b2ljZV90eXBlIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzEiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfMiIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV8zIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfNSIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV82IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzciIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfOCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV85IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InJlbV9saW5lXzEiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icmVtX2xpbmVfMiIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJyZW1fbGluZV8zIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InJlbV9saW5lXzQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icmVtX2xpbmVfNSIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJyZW1fbGluZV82IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InBpcGVsaW5lX2NkIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InN0YXRpb25fbmJyIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InN0YXRpb25fbmFtZSIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJhZGp1c3RtZW50IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImFjdHVhbF9kcnlfbW1idHUiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0iY29tcG9uZW50X3ByaWNlIiBUeXBlPSJEb3VibGUiIC8+PEZpZWxkIE5hbWU9ImNvbXBvbmVudF92YWwiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0iZnJvbV9kYXRlIiBUeXBlPSJEYXRlVGltZSIgLz48RmllbGQgTmFtZT0idG9fZGF0ZSIgVHlwZT0iRGF0ZVRpbWUiIC8+PEZpZWxkIE5hbWU9InVuaXRzIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InByb2RfbW9fdHh0IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InBtbnRfZGF0ZV90eHQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0ibWt0aW5mbyIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJta3RpbmZvMiIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJpc19vcGVuIiBUeXBlPSJCeXRlIiAvPjxGaWVsZCBOYW1lPSJhY3R1YWxfbWNmXzE0NjUiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0ib2xkX2NvbnRyYWN0X25iciIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJhY2N0Z190cmFuc19zZXFfbmJyIiBUeXBlPSJJbnQzMiIgLz48RmllbGQgTmFtZT0idG90YWxfbW1idHVfZHJ5IiBUeXBlPSJEZWNpbWFsIiAvPjxGaWVsZCBOYW1lPSJ0b3RhbF9tbWJ0dV9zYXQiIFR5cGU9IkRlY2ltYWwiIC8+PEZpZWxkIE5hbWU9InRvdGFsX21jZiIgVHlwZT0iRGVjaW1hbCIgLz48RmllbGQgTmFtZT0ic2hvd192b2wiIFR5cGU9IkJ5dGUiIC8+PC9WaWV3PjxWaWV3IE5hbWU9IlF1ZXJ5Ij48RmllbGQgTmFtZT0iaW52b2ljZV9uYnIiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iaW52b2ljZV9yZXYiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iQ29sdW1uMyIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJwcmV2aW91c19hbW91bnQiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0icHJldmlvdXNfYW1vdW50X3R4dCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJ0b3RhbF9hbW91bnQiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0iY29udHJhY3RfbmJyIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eWVyX2JhX25iciIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJwbW50X3Rlcm1zX2RhdGUiIFR5cGU9IkRhdGVUaW1lIiAvPjxGaWVsZCBOYW1lPSJwbW50X3Rlcm1zX3R4dCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJpbnZvaWNlX2RhdGUiIFR5cGU9IkRhdGVUaW1lIiAvPjxGaWVsZCBOYW1lPSJpbnZvaWNlX3R5cGUiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfMSIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV8yIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzMiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfNCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV81IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzYiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYnV5X2xpbmVfNyIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJidXlfbGluZV84IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImJ1eV9saW5lXzkiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icmVtX2xpbmVfMSIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJyZW1fbGluZV8yIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InJlbV9saW5lXzMiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icmVtX2xpbmVfNCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJyZW1fbGluZV81IiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9InJlbV9saW5lXzYiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icGlwZWxpbmVfY2QiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0ic3RhdGlvbl9uYnIiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0ic3RhdGlvbl9uYW1lIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImFkanVzdG1lbnQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0iYWN0dWFsX2RyeV9tbWJ0dSIgVHlwZT0iRG91YmxlIiAvPjxGaWVsZCBOYW1lPSJjb21wb25lbnRfcHJpY2UiIFR5cGU9IkRvdWJsZSIgLz48RmllbGQgTmFtZT0iY29tcG9uZW50X3ZhbCIgVHlwZT0iRG91YmxlIiAvPjxGaWVsZCBOYW1lPSJmcm9tX2RhdGUiIFR5cGU9IkRhdGVUaW1lIiAvPjxGaWVsZCBOYW1lPSJ0b19kYXRlIiBUeXBlPSJEYXRlVGltZSIgLz48RmllbGQgTmFtZT0idW5pdHMiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icHJvZF9tb190eHQiIFR5cGU9IlN0cmluZyIgLz48RmllbGQgTmFtZT0icG1udF9kYXRlX3R4dCIgVHlwZT0iU3RyaW5nIiAvPjxGaWVsZCBOYW1lPSJta3RpbmZvIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9Im1rdGluZm8yIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImlzX29wZW4iIFR5cGU9IkJ5dGUiIC8+PEZpZWxkIE5hbWU9ImFjdHVhbF9tY2ZfMTQ2NSIgVHlwZT0iRG91YmxlIiAvPjxGaWVsZCBOYW1lPSJvbGRfY29udHJhY3RfbmJyIiBUeXBlPSJTdHJpbmciIC8+PEZpZWxkIE5hbWU9ImFjY3RnX3RyYW5zX3NlcV9uYnIiIFR5cGU9IkludDMyIiAvPjxGaWVsZCBOYW1lPSJ0b3RhbF9tbWJ0dV9kcnkiIFR5cGU9IkRlY2ltYWwiIC8+PEZpZWxkIE5hbWU9InRvdGFsX21tYnR1X3NhdCIgVHlwZT0iRGVjaW1hbCIgLz48RmllbGQgTmFtZT0idG90YWxfbWNmIiBUeXBlPSJEZWNpbWFsIiAvPjxGaWVsZCBOYW1lPSJzaG93X3ZvbCIgVHlwZT0iQnl0ZSIgLz48L1ZpZXc+PC9EYXRhU2V0PjwvUmVzdWx0U2NoZW1hPjxDb25uZWN0aW9uT3B0aW9ucyBDbG9zZUNvbm5lY3Rpb249InRydWUiIC8+PC9TcWxEYXRhU291cmNlPg==\"/>");
        WriteEndObject("</ComponentStorage>");
    }

    private static int X(object value)
    {
        if (!Int32.TryParse((string)value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as Int32");
        }

        return PBFormattingHelper.ConvertX(numValue);
    }

    private static int Y(object value)
    {
        if (!Int32.TryParse((string)value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as Int32");
        }

        return PBFormattingHelper.ConvertY(numValue);
    }

    private static string Color(object value)
    {
        if (!Int32.TryParse((string)value, out var numValue))
        {
            throw new Exception($"Couldn't parse value: {value} as Int32");
        }
        return PBFormattingHelper.ConvertColor(numValue);
    }

    private static string Alignment(object value)
    {
        return PBFormattingHelper.ConvertAlignment((string)value);
    }

    private static string FixFormattingString(object value)
    {
        return ((string)value).ToLower().Replace("mm", "MM");
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
