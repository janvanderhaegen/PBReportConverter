using ReportMigration.Converters;

using ReportMigration.Parser;

//var curDirectory = Directory.GetCurrentDirectory();
//var projectPath = curDirectory[..curDirectory.IndexOf("\\bin")];

////var parser = new PBReportParser(Path.Combine(projectPath, "PBReports\\TestReport.pbl"));

////parser.Parse();

////var result = parser.GetStructure();
//var fileName = "d_pop_stmt";

//var converter = new PblToRepxConverter(Path.Combine(projectPath, "PBReports"), Path.Combine(projectPath, $"RepxReports\\{fileName}.repx"));

//converter.GenerateRepxFile($"{fileName}.p");

var parser = new PBExpressionParser(" round(whd_vol / mcf_whd_vol ,4)");

var result = parser.Parse();

var a = 1;
