using ReportMigration.Parser;

var parser = new PBReportParser("C:\\Users\\Tadija\\source\\repos\\ReportMigration\\Reports\\TestReport.pbl");
parser.Parse();

var result = parser.GetStructure();

Console.WriteLine(result);
