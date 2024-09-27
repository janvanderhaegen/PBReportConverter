using PBReportConverter.Converters;
using ReportMigration.Converters;

Console.WriteLine("Enter path of directory with PowerBuilder files:");
var pbPath = Console.ReadLine();

//check if input directory exists
var pbInfo = new DirectoryInfo(pbPath!);
if (!pbInfo.Exists)
{
    Console.WriteLine("Input directory doesn't exist");
    return;
}
pbPath = pbInfo.FullName; 

//if there are any pbl files, convert them
var pebbles = Directory.GetFiles(pbPath!, "*.pbl", SearchOption.AllDirectories);
Console.WriteLine($"Found {pebbles.Length} pbl files");
var pblToSrdConverter = new PblToSrdConverter();
foreach (var pebble in pebbles)
{
    pblToSrdConverter.Unpack(pebble);
}
if (pebbles.Any())
    return;

//else, convert srd files to repx
Console.WriteLine("Enter path of target directory:");
var repxPath = Console.ReadLine();
var repxInfo = new DirectoryInfo(repxPath!);
if (!repxInfo.Exists)
{
    repxInfo.Create();
}
repxPath = repxInfo.FullName;


var srdFiles = new[] { "*.srd", "*.p" }.SelectMany(pattern => Directory.GetFiles(pbPath!, pattern, SearchOption.AllDirectories)).ToArray();
Console.WriteLine($"Found {srdFiles.Length} .srd|.p files");
var converter = new SrdToRepxConverter(pbPath!, repxPath!);
foreach (var fileName in srdFiles)
{
    var relativePath = Path.GetRelativePath(pbPath!, fileName);
    converter.GenerateRepxFile(relativePath);
}