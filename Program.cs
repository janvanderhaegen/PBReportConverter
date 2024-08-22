using ReportMigration.Converters;
using ReportMigration.Parser;

//var parser = new PBReportParser("C:\\Users\\Tadija\\source\\repos\\ReportMigration\\Reports\\TestReport.pbl");
//parser.Parse();

//var result = parser.GetStructure();

//Console.WriteLine(result);

var converter = new PblToRepxConverter("C:\\Users\\Tadija\\source\\repos\\ReportMigration\\Reports\\TestReport.pbl", "C:\\Users\\Tadija\\source\\repos\\ReportMigration\\Reports\\TestReport.repx");

converter.GenerateRepxFile();
