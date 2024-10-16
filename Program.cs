using PBReportConverter.Converters;
using ReportMigration.Converters;

Console.WriteLine("Enter path of directory with PowerBuilder files:");
var inputPath =Console.ReadLine();

//check if input directory exists
var pbInfo = new DirectoryInfo(inputPath!);
if (!pbInfo.Exists)
{
    Console.WriteLine("Input directory doesn't exist");
    return;
}
inputPath = pbInfo.FullName;

//if there are any pbl files, convert them
var pebbles = Directory.GetFiles(inputPath!, "*.pbl", SearchOption.AllDirectories);
Console.WriteLine($"Found {pebbles.Length} pbl files");
var pblToSrdConverter = new PblToSrdConverter();
foreach (var pebble in pebbles)
{
    pblToSrdConverter.Unpack(pebble);
}
if (pebbles.Any())
{
    Console.WriteLine($"Unpacked {pebbles.Length} pbl files");
    return;
}

//else, convert srd files to repx
Console.WriteLine("Enter path of target directory:");
var outputPath = Console.ReadLine();
var repxInfo = new DirectoryInfo(outputPath!);
if (!repxInfo.Exists)
{
    repxInfo.Create();
}
outputPath = repxInfo.FullName;


var srdFiles = new[] { "*.srd", "*.p" }.SelectMany(pattern => Directory.GetFiles(inputPath!, pattern, SearchOption.AllDirectories)).ToArray();
Console.WriteLine($"Found {srdFiles.Length} .srd|.p files");
var converter = new SrdToRepxConverter(inputPath!, outputPath!);
foreach (var fileName in srdFiles)
{
    var relativePath = Path.GetRelativePath(inputPath!, fileName);
    converter.GenerateRepxFile(relativePath);
}
if (srdFiles.Any())
{
    Console.WriteLine($"Converted {srdFiles.Length} srd files to repx");
    return;
}

//else, convert repx files to json
var repxFiles = Directory.GetFiles(inputPath!, "*.repx", SearchOption.AllDirectories).ToArray();
Console.WriteLine($"Found {repxFiles.Length} .repx files");
var repxToJsonConverter = new RepxToJsonConverter(inputPath!, outputPath!);
foreach (var fileName in repxFiles)
{
    var relativePath = Path.GetRelativePath(inputPath!, fileName);
    repxToJsonConverter.ConvertToJson(relativePath);
}
if (repxFiles.Any())
{
    Console.WriteLine($"Converted {repxFiles.Length} repx files to json");
    return;
}