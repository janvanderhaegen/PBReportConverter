using ReportMigration.Converters;

Console.WriteLine("Enter path of directory with PowerBuilder files:");
var pbPath = Console.ReadLine();

Console.WriteLine("Enter path of target directory:");
var repxPath = Console.ReadLine();

var files = Directory.GetFiles(pbPath!).Select(f => Path.GetFileName(f));

foreach (var fileName in files)
{
    var converter = new PblToRepxConverter(pbPath!, repxPath!);
    converter.GenerateRepxFile(fileName);
}