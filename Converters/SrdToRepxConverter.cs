using PBReportConverter.Models;
using PBReportConverter.Parser;
using static PBReportConverter.Helpers.DataSourceXmlGenerator;
using static PBReportConverter.Helpers.PBFormattingHelper;
using PBReportConverter.Helpers;

namespace PBReportConverter.Converters;

internal class SrdToRepxConverter(string inputDir, string outputDir)
{
    private int _ref = 1;
    private StreamWriter? _writer;
    private readonly string _inputDir = inputDir;
    private readonly string _outputDir = outputDir;
    private int _tabulator = 0;
    private int _groupCount;
    private double _globalHeight;
    private double _globalWidth;
    private readonly Dictionary<string, int> _globalParams = [];
    private List<(string name, string expression)> _currentComputes = [];
    private List<string> _currentColumns = [];

    // Attributes which may have expressions tied to their value
    private static readonly List<string> _expressionableAttributes = ["visible", "height", "width"];

    // Offset is added to account for font options that are not present in DevExpress, causing some text to not fit into the usual dimensions by a couple of pixels
    private static readonly int _labelWidthOffset = 3;

    /// <summary>
    /// Main public method used to start the conversion of the given file path.
    /// The method first parses the source file via the initialized PBReportParser, and stores the result into a structure of ContainerModel objects.
    /// It then initializes the "_global" and "_current" variables, before generating the data source and body of the resulting .repx file.
    /// </summary>
    /// <param name="fileName"></param>
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

            WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            var dataWindowIndex = structure.FindIndex(x => x._objectType == "datawindow");
            var dataWindowAttributes = structure[dataWindowIndex]._attributes;
            WriteStartObject($"<XtraReportsLayoutSerializer SerializerVersion=\"24.1.4.0\" Ref=\"{_ref++}\" ControlType=\"DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v24.1, Version=24.1.4.0, Culture=neutral\" Name=\"XtraReport1\" VerticalContentSplitting=\"Smart\" Margins=\"{X(dataWindowAttributes["print.margin.left"])}, {X(dataWindowAttributes["print.margin.right"])}, {Y(dataWindowAttributes["print.margin.top"])}, {Y(dataWindowAttributes["print.margin.bottom"])}\" PaperKind=\"Custom\" PageWidth=\"{parser.ReportWidth + X(dataWindowAttributes["print.margin.left"]) + X(dataWindowAttributes["print.margin.right"])}\" PageHeight=\"{parser.ReportHeight + Y(dataWindowAttributes["print.margin.top"]) + Y(dataWindowAttributes["print.margin.bottom"]) + 200}\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-0\">");

            // Extracts the single TableModel object from the structure, to be used for extracting column names and generating the Data Source encoded XML.
            var tableContainer = (TableModel)structure.Where(x => x.GetType() == typeof(TableModel)).ToList()[0];

            _currentColumns = GetColumns(tableContainer);

            // Generates input parameters of the report, if there are any present.
            if (tableContainer._attributes.TryGetValue("arguments", out string? value))
                GenerateParameters(GetParameters(value));

            GenerateDataSource(tableContainer, 0);
            GenerateBody(structure, dataWindowIndex);

            WriteEndObject("</XtraReportsLayoutSerializer>");

            _writer.Flush();
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
        finally
        {
            // Disposes of the writer and resets the state of the converter, to be ready for the next input file.
            _writer?.Dispose();

            _ref = 1;
            _tabulator = 0;
            _globalParams.Clear();
            _currentComputes.Clear();
            _currentColumns.Clear();
            Console.Write(" - Done\n");
        }
    }

    /// <summary>
    /// Fetches list of column names from the parsed TableModel, as they are named in the database.
    /// If the database and PowerBuilder names are different, it also adds the name mapping to the list of compute values, to be added later in the report.
    /// As the PowerBuilder elements are using the PowerBuilder column name, but the Data Source needs the original name as it is in the database,
    /// it's necessary to create this computed value, so as to not break any Text Label links to the Table Columns.
    /// </summary>
    /// <param name="tableContainer"></param>
    /// <returns></returns>
    private List<string> GetColumns(TableModel tableContainer)
    {
        var columns = new List<string>();

        foreach(var elem in tableContainer._columns)
        {
            var name = elem._attributes["name"];
            var dbname = elem._attributes["dbname"];
            
            // Removes the table name from the column name, if present.
            var tableNameEndIndex = dbname.IndexOf('.');
            if (tableNameEndIndex > 0)
            {
                dbname = dbname[(tableNameEndIndex + 1)..];
            }

            columns.Add(elem._attributes["name"]);
            if(dbname != name)
            {
                _currentComputes.Add((name, dbname));
            }
        }
        return columns;
    }

    /// <summary>
    /// Generates the header, footer and detail bands of the report, as well as any existing group headers and footers.
    /// After this, it generates all the calculated fields from the _currentComputes list, which gets filled up within the methods called by GenerateBody().
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="dataWindowIndex"></param>
    private void GenerateBody(List<ContainerModel> structure, int dataWindowIndex)
    {
        var dataReportAttributes = structure[dataWindowIndex]._attributes;
        WriteStartObject("<Bands>");
        int itemCounter = 1;
        WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"TopMarginBand\" Name=\"TopMargin\" HeightF=\"{Y(dataReportAttributes["print.margin.top"])}\"/>");
        WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"BottomMarginBand\" Name=\"BottomMargin\" HeightF=\"{Y(dataReportAttributes["print.margin.bottom"])}\"/>");
        foreach (var container in structure[1..])
        {
            switch (container._objectType)
            {
                case "group":
                    {
                        GenerateGroupBands(container, ref itemCounter);
                        break;
                    }
                case "table":
                    {
                        break;
                    }
                default:
                    {
                        GenerateBand(container, ref itemCounter);
                        break;
                    }
            }
        }

        WriteEndObject("</Bands>");

        WriteStartObject("<CalculatedFields>");
        itemCounter = 1;
        foreach (var (name, expression) in _currentComputes)
        {
            WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{name}\" Expression=\"{expression}\" DataMember=\"Query\" />");
        }
        WriteEndObject("</CalculatedFields>");
        // Resets the computes after they've been used.
        _currentComputes.Clear();
    }

    /// <summary>
    /// Generates the SubReport specified within the main report.
    /// The SubReport needs its entire .repx specification written inside of the main report's body,
    /// so multiple tmp variables are added to preserve the parent report's configuration state until SubReport generation is completed.
    /// The method creates a new parser for the SubReport's source file and repeats all the steps present in the generation of the main report.
    /// After this, it generates the parameter bindings between the input parameters of the SubReport and the input parameters OR result columns of the main report.
    /// At the end, it returns the configuration state to that of the parent report.
    /// </summary>
    /// <param name="subreport"></param>
    /// <param name="itemCounter"></param>
    /// <exception cref="Exception">Thrown in file specified in the SubReport's attributes isn't present in the _inputDir.</exception>
    private void GenerateSubReport(ObjectModel subreport, ref int itemCounter)
    {
        var objType = ConvertElementType(subreport._objectType);
        var attributes = subreport._attributes;

        // Generates a transparent rectangle around the SubReport if the border attribute is present
        if (attributes.TryGetValue("border", out var borderStr) && int.TryParse(borderStr, out var border) && border > 0)
        {
            GenerateBorder(attributes, border, ref itemCounter);
        }

        // Preserving the parent report's configuration state in tmp variables
        var tmpHeight = _globalHeight;
        var tmpWidth = _globalWidth;
        var tmpGroupCount = _groupCount;
        var tmpUom = uom;
        var tmpComputes = new List<(string name, string expression)>(_currentComputes);
        var tmpColumns = new List<string>(_currentColumns);

        // Fetches the first .srd or .p file with the name specified as the SubReport
        var subreportPath = string.Empty;
        var files = new[] { $"{attributes["dataobject"]}.srd", $"{attributes["dataobject"]}.p" }.SelectMany(pattern => Directory.GetFiles(_inputDir, pattern, SearchOption.AllDirectories)).ToArray();
        if (files.Length > 0)
        {
            subreportPath = files[0];
        }

        if (subreportPath.Length == 0)
        {
            throw new FileNotFoundException($"Source file doesn't exist for subreport: {attributes["dataobject"]}");
        }

        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["dataobject"]}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\">");

        PBReportParser parser = new(subreportPath);
        var structure = parser.Parse();

        // Sets configuration state to that of the SubReport
        _globalHeight = parser.ReportHeight;
        _globalWidth = parser.ReportWidth;
        _groupCount = parser.GroupCount;
        _currentComputes.Clear();

        var dataWindowIndex = structure.FindIndex(x => x._objectType == "datawindow");
        var dataWindowAttributes = structure[dataWindowIndex]._attributes;
        var dataSourceRef = _ref++;

        WriteStartObject($"<ReportSource Ref=\"{_ref++}\" Name=\"{attributes["dataobject"]}\" ControlType=\"ReportMigration.XtraReports.{attributes["dataobject"]}, ReportMigration, Version=1.0.0.0, Culture=neutral\" VerticalContentSplitting=\"Smart\" Margins=\"{X(dataWindowAttributes["print.margin.left"])}, {X(dataWindowAttributes["print.margin.right"])}, {Y(dataWindowAttributes["print.margin.top"])}, {Y(dataWindowAttributes["print.margin.bottom"])}\" PaperKind=\"Custom\" PageWidth=\"{parser.ReportWidth}\" PageHeight=\"{parser.ReportHeight}\" Version=\"24.1\" DataMember=\"Query\" DataSource=\"#Ref-{dataSourceRef}\">");

        var tableContainer = (TableModel)structure.Where(x => x.GetType() == typeof(TableModel)).ToList()[0];
        _currentColumns = GetColumns(tableContainer);
        var argList = GetParameters(tableContainer._attributes["arguments"]);

        GenerateParameters(argList);
        GenerateDataSource(tableContainer, dataSourceRef);
        GenerateBody(structure, dataWindowIndex);

        WriteEndObject("</ReportSource>");

        // nest_arguments are the arguments passed to the SubReport as input parameters.
        if(attributes.TryGetValue("nest_arguments", out var argsString))
        {
            var parameterArguments = GetParameters(attributes["nest_arguments"]);
            WriteStartObject("<ParameterBindings>");
            var subItemCounter = 0;
            foreach (var arg in parameterArguments)
            {
                var paramName = argList[subItemCounter++];
                // Checks if input parameter is present in the parent report and binds it to its reference number
                if (_globalParams.TryGetValue(arg, out var refNum))
                {
                    WriteLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" Parameter=\"#Ref-{refNum}\" />");
                }
                // Otherwise, it's bound to a column from the result table
                else
                {
                    WriteLine($"<Item{subItemCounter} Ref=\"{_ref++}\" ParameterName=\"{paramName}\" DataMember=\"Query.{arg}\" />");
                }
            }
            WriteEndObject("</ParameterBindings>");
        }

        WriteEndObject($"</Item{itemCounter++}>");

        // Restores parent configuration state
        _globalHeight = tmpHeight;
        _globalWidth = tmpWidth;
        _groupCount = tmpGroupCount;
        uom = tmpUom;
        _currentComputes = tmpComputes;
        _currentColumns = tmpColumns;
    }

    private void GenerateBorder(Dictionary<string, string> attributes, int border, ref int itemCounter)
    {
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"XRShape\" Name=\"border_{itemCounter}\" LineWidth=\"{border}\" SizeF=\"{X(attributes["width"]) + _labelWidthOffset},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\">");
        WriteLine($"<Shape Ref=\"{_ref++}\" ShapeName=\"Rectangle\" />");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    /// <summary>
    /// Generates the list of input parameters and, if not already present, adds them to the list of known parameters and their reference numbers,
    /// to be used in SubReport parameter bindings
    /// </summary>
    /// <param name="arguments"></param>
    private void GenerateParameters(List<string> arguments)
    {
        WriteStartObject("<Parameters>");
        var itemCounter = 1;
        foreach (var element in arguments)
        {
            _globalParams.TryAdd(element, _ref);
            WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" Name=\"{element}\"/>");
        }
        WriteEndObject("</Parameters>");
    }

    /// <summary>
    /// Generates the given report band.
    /// Generates all the text elements listed within its specifications first, after which it generates the shapes,
    /// so as to make sure all the background shapes are shown behind the text.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="itemCounter"></param>
    private void GenerateBand(ContainerModel container, ref int itemCounter)
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

        // The PowerBuilder summary band is converted into the GroupFooter band.
        // The GroupFooter's level must be higher than any of the actual Group bands present in the report,
        // to make sure that it's underneath all of them, as the summary band is supposed to be at the bottom of the report.
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
                GenerateElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    // Adds the Visible=False attribute to a band, only if its height is set to 0
    private static string SetVisible(double height)
    {
        if (height > 0)
        {
            return "";
        }
        return " Visible=\"False\"";
    }

    /// <summary>
    /// PowerBuilder reports have a single group object for both the header and footer bands, the attributes of which are prefixed with header. and trailer. respectively.
    /// This method generates the header and footer bands separately based on the specifications from the original group object.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="itemCounter"></param>
    /// <exception cref="Exception"></exception>
    private void GenerateGroupBands(ContainerModel container, ref int itemCounter)
    {
        var attributes = container._attributes;
        var elements = container._elements;

        // Gets the group's level.
        if (!int.TryParse(attributes["level"], out var level))
        {
            throw new Exception($"Group element {attributes["name"]} is missing level attribute");
        }

        int subItemCounter = 1;
        var backgroundShapes = new List<ObjectModel>();

        // Generates Group Header.
        var height = Y(attributes["header.height"]);
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupHeaderBand\" Name=\"groupHeaderBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{height}\"{SetVisible(height)}>");

        // Generates the fields by which the data is grouped by.
        var groupBy = attributes["by"].Trim('(', ')').Split(',');
        WriteStartObject("<GroupFields>");
        foreach (var elem in groupBy)
        {
            WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" FieldName=\"{elem}\" />");
        }
        subItemCounter = 1;
        WriteEndObject("</GroupFields>");
        WriteStartObject("<Controls>");
        // Filters elements of the group for those that contain the prefix "header"
        foreach (var element in elements.Where(x => x._attributes["band"].Contains("header")))
        {
            var objType = ConvertElementType(element._objectType);
            if (objType == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");

        // Generates Group Footer.
        height = Y(attributes["trailer.height"]);
        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"GroupFooterBand\" Name=\"groupFooterBand{attributes["level"]}\" Level=\"{_groupCount - level}\" HeightF=\"{height}\"{SetVisible(height)}>");
        WriteStartObject("<Controls>");
        subItemCounter = 1;
        backgroundShapes.Clear();
        // Filters elements of the group for those that contain the prefix "trailer"
        foreach (var element in elements.Where(x => x._attributes["band"].Contains("trailer")))
        {
            if (ConvertElementType(element._objectType) == "XRShape")
            {
                backgroundShapes.Add(element);
            }
            else
            {
                GenerateElement(element, ref subItemCounter, container);
            }
        }
        foreach (var element in backgroundShapes)
        {
            GenerateElement(element, ref subItemCounter, container);
        }
        WriteEndObject("</Controls>");
        WriteEndObject($"</Item{itemCounter++}>");
    }

    /// <summary>
    /// Generates an element within a band.
    /// The method first determines the element's position and dimensions within the container band, and sets its visibility to False if it exceeds the band's dimensions.
    /// After this, it checks if any of the attributes from _expressionableAttributes have expression definitions within them, and adds them to a list for use in expression bindings.
    /// Based on the object's type, it generates the appropriate specifications and expression bindings.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="itemCounter"></param>
    /// <param name="container"></param>
    private void GenerateElement(ObjectModel element, ref int itemCounter, ContainerModel container)
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

        // The following block of code is for determining where the element fits within the container,
        // and if it should be visible (in case it's positioned outside of it)
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
        if (attributes.TryGetValue("visible", out var value) && value == "0")
        {
            visibility = false;
        }
        else
        {
            visibility = x < _globalWidth && y < containerHeight;
        }

        var nameAttr = string.Empty;

        if(attributes.TryGetValue("name", out var name))
        {
            nameAttr = $"Name=\"{name}_label\"";
        }

        attributes.TryGetValue("format", out var formatString);

        // Checks if expressionable attributes have expressions in them.
        List<(string attr, (string printEvent, string expr))> attrExpressions = [];
        foreach(var attr in _expressionableAttributes)
        {
            if(attributes.TryGetValue(attr, out var attrValue))
            {
                var (printEvent, fixedExpression) = CheckForExpressionString(attrValue);
                if(fixedExpression != string.Empty)
                {
                    foreach (var param in _globalParams.Keys)
                    {
                        if (fixedExpression.Contains(param))
                        {
                            fixedExpression = fixedExpression.Replace(param, '?' + param);
                            break;
                        }
                    }
                    if (attr == "visible")
                    {
                        fixedExpression = fixedExpression.Replace(",1", ",true").Replace(",0", ",false");
                    }

                    attrExpressions.Add(new(MapAttr(attr), (printEvent, fixedExpression)));
                }
            }
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
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" {nameAttr} TextFormatString=\"{FixFormattingString(formatString)}\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\">");
                    var colFilterList = _currentColumns.Where(col => attributes["name"].Contains(col)).ToList();

                    // We search for the longest existing column name that can fit into the new column
                    // Repeated columns in PowerBuilder follow the naming structure of [column_name]_[number_of_repeats]
                    // For example, a second usage of the invoice_nbr column would be called invoice_nbr_1, third would be invoice_nbr_2 etc.
                    var expr = colFilterList.Count > 0 ? colFilterList.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur) : attributes["name"];
                    
                    WriteStartObject("<ExpressionBindings>");
                    var subItemCounter = 1;
                    WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"BeforePrint\" PropertyName=\"Text\" Expression=\"[{expr}]\"/>");
                    foreach(var (attr, (prEvent, prExpr)) in attrExpressions)
                    {
                        WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"{prEvent}\" PropertyName=\"{attr}\" Expression=\"{prExpr}\"/>");
                    }
                    WriteEndObject("</ExpressionBindings>");

                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
            case "text":
                {
                    var text = attributes["text"].Replace('&', '-');

                    var textItemDef = $"Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" {nameAttr} TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\" Multiline=\"true\" Text=\"{text.Split("~t")[0]}\" SizeF=\"{X(attributes["width"]) + _labelWidthOffset},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\"";

                    var (printEvent, fixedExpression) = CheckForExpressionString(text);
                    if (attrExpressions.Count > 0 || fixedExpression != string.Empty)
                    {
                        foreach(var param in _globalParams.Keys)
                        {
                            // If an input parameter is used in the expression, it needs to be prefixed with '?'
                            if (fixedExpression.Contains(param))
                            {
                                fixedExpression = fixedExpression.Replace(param, '?' + param);
                                break;
                            }
                        }
                        WriteStartObject($"<{textItemDef}>");
                        WriteStartObject("<ExpressionBindings>");
                        var subItemCounter = 1;

                        if (fixedExpression != string.Empty)
                        {
                            WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"{printEvent}\" PropertyName=\"Text\" Expression=\"{fixedExpression}\"/>");
                        }

                        foreach (var (attr, (prEvent, prExpr)) in attrExpressions)
                        {
                            WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"{prEvent}\" PropertyName=\"{attr}\" Expression=\"{prExpr}\"/>");
                        }

                        WriteEndObject("</ExpressionBindings>");
                        WriteEndObject($"</Item{itemCounter++}>");
                    }
                    else
                    {
                        WriteLine($"<{textItemDef}/>");
                        itemCounter++;
                    }
                    break;
                }
            case "compute":
                {
                    // There is no expression for a page number in DevExpress, so instead we add the XRPageInfo element, which displays the page number.
                    if(attributes["expression"] == "page()")
                    {
                        WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"XRPageInfo\" {nameAttr} PageInfo=\"Number\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" />");
                    }
                    else
                    {
                        var baseExpr = attributes["expression"];
                        var (printEvent, expression) = Expression(baseExpr);
                        foreach (var param in _globalParams.Keys)
                        {
                            // If an input parameter is used in the expression, it needs to be prefixed with '?'
                            if (expression.Contains(param))
                            {
                                expression = expression.Replace(param, '?' + param);
                                break;
                            }
                        }
                        WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{name}_field\" TextFormatString=\"{FixFormattingString(formatString)}\" TextAlignment=\"{ConvertAlignment(attributes["alignment"])}\"  Multiline=\"true\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" AnchorVertical=\"Both\" Font=\"{attributes["font.face"]}, {attributes["font.height"][1..]}pt{CheckBold(attributes["font.weight"])}\" Visible=\"{visibility}\">");
                        if(container._objectType == "group" && baseExpr.Contains("cumulative"))
                        {
                            WriteLine($"<Summary Ref=\"{_ref++}\" Running=\"Group\" />");
                        }
                        WriteStartObject("<ExpressionBindings>");
                        var subItemCounter = 1;
                        WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"{printEvent}\" PropertyName=\"Text\" Expression=\"{(printEvent == "BeforePrint" ? $"[{name}]": expression)}\"/>");
                        foreach (var (attr, (prEvent, prExpr)) in attrExpressions)
                        {
                            WriteLine($"<Item{subItemCounter++} Ref=\"{_ref++}\" EventName=\"{prEvent}\" PropertyName=\"{attr}\" Expression=\"{prExpr}\"/>");
                        }
                        WriteEndObject("</ExpressionBindings>");

                        WriteEndObject($"</Item{itemCounter++}>");
                        if(printEvent == "BeforePrint")
                            _currentComputes.Add((name!, expression));
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
                    WriteLine($"<Item{itemCounter++} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(color)}\" SizeF=\"{length},{height}\" LocationFloat=\"{x},{y}\"  LineDirection=\"{direction}\" Visible=\"{visibility}\"/>");
                    break;
                }
            case "rectangle":
                {
                    if (!attributes.TryGetValue("background.gradient.color", out var color))
                    {
                        color = "";
                    }
                    WriteStartObject($"<Item{itemCounter} Ref=\"{_ref++}\" ControlType=\"{objType}\" Name=\"{attributes["name"]}\" FillColor=\"{Color(color)}\" SizeF=\"{X(attributes["width"])},{Y(attributes["height"])}\" LocationFloat=\"{X(attributes["x"])},{Y(attributes["y"])}\" Visible=\"{visibility}\">");
                    WriteLine($"<Shape Ref=\"{_ref++}\" ShapeName=\"Rectangle\"/>");
                    WriteEndObject($"</Item{itemCounter++}>");
                    break;
                }
        }
    }

    private void GenerateDataSource(TableModel table, int dataSourceRef)
    {
        WriteStartObject("<ComponentStorage>");
        WriteLine($"<Item1 Ref=\"{dataSourceRef}\" ObjectType=\"DevExpress.DataAccess.Sql.SqlDataSource,DevExpress.DataAccess.v24.1\" Name=\"sqlDataSource1\" Base64=\"{GenerateDataSourceXML(table)}\"/>");
        WriteEndObject("</ComponentStorage>");
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
        WriteLine(str);
        _tabulator++;
    }

    private void WriteEndObject(string str)
    {
        _tabulator--;
        WriteLine(str);
    }

    private void WriteLine(string str)
    {
        for (var i = 0; i < _tabulator; i++)
        {
            _writer!.Write("\t");
        }
        _writer!.WriteLine(str);
    }
}
