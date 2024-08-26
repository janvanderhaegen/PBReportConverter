using ReportMigration.Converters;

var curDirectory = Directory.GetCurrentDirectory();
var projectPath = curDirectory.Substring(0, curDirectory.IndexOf("\\bin"));

var converter = new PblToRepxConverter(Path.Combine(projectPath, "PBReports\\TestReport.pbl"), Path.Combine(projectPath, "RepxReports\\TestReport.repx"));

converter.GenerateRepxFile();
