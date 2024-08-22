using ReportMigration.Models;
using ReportMigration.Parser;
using System.ComponentModel;
using System.Globalization;

namespace ReportMigration.Converters;

internal class PblToRepxConverter
{
    private List<ContainerModel> _structure;
    private int _ref = 1;
    private StreamWriter _writer;

    public PblToRepxConverter(string inputPath, string outputPath)
    {
        PBReportParser parser = new PBReportParser(inputPath);
        parser.Parse();
        _structure = parser.GetStructure();
        _writer = new StreamWriter(outputPath);
    }

    public void GenerateRepxFile()
    {
        _writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        var dataReportAttributes = _structure[0]._attributes;
        _writer.WriteLine($"<XtraReportsLayoutSerializer SerializerVersion=\"24.1.4.0\" Ref=\"{_ref++}\" ControlType=\"DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v24.1, Version=24.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a\" Name=\"XtraReport2\" VerticalContentSplitting=\"Smart\" Margins=\"{dataReportAttributes["print.margin.left"]}, {dataReportAttributes["print.margin.right"]}, {dataReportAttributes["print.margin.top"]}, {dataReportAttributes["print.margin.bottom"]}\" PageWidth=\"850\" PageHeight=\"1100\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-0\" Font=\"Arial, 9.75pt\">");
        _writer.WriteLine("<Bands>");
        int itemCounter = 1;
        _writer.WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"TopMarginBand\" Name=\"TopMargin\" />");
        _writer.WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"BottomMarginBand\" Name=\"BottomMargin\" />");
        foreach(var container in _structure[1..])
        {
            if(container._objectType != "table" && container._objectType != "summary")
            {
                GenerateElement(container, ref itemCounter);
            }
        }

        _writer.WriteLine("</Bands>");
        _writer.WriteLine("</XtraReportsLayoutSerializer>");
        _writer.Flush();
    }

    public void GenerateElement(ContainerModel container, ref int itemCounter)
    {
        var objType = ControlTypeConverter(container._objectType);
        if (objType == null)
        {
            return;
        }
        var attributes = container._attributes;
        var elements = container._elements;
        if(objType != "group")
        {
            _writer.WriteLine($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}Band\" Name=\"{objType}\" HeightF=\"{attributes["height"]}\">");
            //int subItemCounter = 1;
            //foreach(var element in elements)
            //{
            //    GenerateSubElement(element, ref subItemCounter);
            //}
            _writer.WriteLine($"</Item{itemCounter++}>");
        }
        else
        {
            _writer.WriteLine($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupHeaderBand\" Name=\"groupHeaderBand{attributes["level"]}\" Level=\"{attributes["level"]}\" HeightF=\"{attributes["header.height"]}\">");
            _writer.WriteLine($"</Item{itemCounter++}>");
            _writer.WriteLine($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupFooterBand\" Name=\"groupFooterBand{attributes["level"]}\" Level=\"{attributes["level"]}\" HeightF=\"{attributes["trailer.height"]}\">");
            _writer.WriteLine($"</Item{itemCounter++}>");
        }
    }

    //public void GenerateSubElement(ObjectModel element, ref int itemCounter)
    //{
    //    var objType = ControlTypeConverter(element._objectType);
    //    if (objType == null)
    //    {
    //        return;
    //    }
    //    var attributes = element._attributes;

    //    switch (element._objectType)
    //    {
    //        case
    //    }
    //}

    public string? ControlTypeConverter(string ctrlType)
    {
        switch (ctrlType)
        {
            case "header": return "ReportHeader";
            case "footer": return "ReportFooter";
            case "detail": return "Detail";
            case "group": return ctrlType;
            case "rectangle": return "XRPanel";
            case "text":
            case "column": return "XRLabel";

            //default: throw new NotImplementedException($"Unsupported type: {ctrlType}");
            default: return null;
        }
    }
}
