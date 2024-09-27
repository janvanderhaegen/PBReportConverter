using PBReportConverter.Converters;
using ReportMigration.Converters;

Console.WriteLine("Enter path of directory with PowerBuilder files:");
var pbPath = Console.ReadLine();
var pbInfo = new DirectoryInfo(pbPath!);
if (!pbInfo.Exists)
{
    Console.WriteLine("Directory doesn't exist");
    return;
}
pbPath = pbInfo.FullName;


Console.WriteLine("Enter path of target directory:");
var repxPath = Console.ReadLine();
var repxInfo = new DirectoryInfo(repxPath!);
if (!repxInfo.Exists)
{
    Console.WriteLine("Directory doesn't exist");
    return;
}
repxPath = repxInfo.FullName;

var pebbles = Directory.GetFiles(pbPath!, "*.pbl", SearchOption.AllDirectories);
Console.WriteLine($"Found {pebbles.Length} pbl files");
var pblToSrdConverter = new PblToSrdConverter();
foreach (var pebble in pebbles)
{
    pblToSrdConverter.Unpack(pebble);
}

var files = Directory.GetFiles(pbPath!).Select(f => Path.GetFileName(f));

var converter = new PblToRepxConverter(pbPath!, repxPath!);
foreach (var fileName in files)
{
    converter.GenerateRepxFile(fileName);
}