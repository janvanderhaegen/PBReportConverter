using PBReportConverter.Converters;
using System;
using static PBReportConverter.Converters.PblToSrdConverter;

Console.WriteLine("Enter path of directory with .pbl, .srd or .repx files");
var inputPath = Console.ReadLine();

//check if input directory exists
var pbInfo = new DirectoryInfo(inputPath!);
if (!pbInfo.Exists)
{
    Console.WriteLine("Input directory doesn't exist");
    return;
}
inputPath = pbInfo.FullName;

var pebbleFiles = Directory.GetFiles(inputPath!, "*.pbl", SearchOption.AllDirectories);
var srdFiles = new[] { "*.srd", "*.p" }.SelectMany(pattern => Directory.GetFiles(inputPath!, pattern, SearchOption.AllDirectories)).Where(file => !file.Contains("_frf")).ToArray();
var repxFiles = Directory.GetFiles(inputPath!, "*.repx", SearchOption.AllDirectories).ToArray();

Console.WriteLine($"Found {pebbleFiles.Length} .pbl, {srdFiles.Length} .pbl, {repxFiles.Length} .repx");

//if there are any pbl files, unpack them
if (pebbleFiles.Any())
{
    Console.WriteLine("Unpackng pbl files...");
    foreach (var pebble in pebbleFiles)
    {
        Unpack(pebble);
    }
    Console.WriteLine($"Unpacked {pebbleFiles.Length} pbl files");
}

//else if there are any srd files, convert them to repx
else if (srdFiles.Any())
{
    Console.WriteLine($"Converting .srd files");
    Console.WriteLine("Enter path of target directory");
    var outputPath = Console.ReadLine();
    var repxInfo = new DirectoryInfo(outputPath!);
    if (!repxInfo.Exists)
    {
        repxInfo.Create();
    }
    outputPath = repxInfo.FullName;
    var converter = new SrdToRepxConverter(inputPath!, outputPath!);
    foreach (var fileName in srdFiles)
    {
        var relativePath = Path.GetRelativePath(inputPath!, fileName);
        converter.GenerateRepxFile(relativePath);
    }
    Console.WriteLine($"Converted {srdFiles.Length} srd files to repx");
}
//else if there are any repx files, convert them to json
else if (repxFiles.Any())
{
    Console.WriteLine($"Converting .srd files");
    Console.WriteLine("Enter path of target directory");
    var outputPath = Console.ReadLine();
    var repxInfo = new DirectoryInfo(outputPath!);
    if (!repxInfo.Exists)
    {
        repxInfo.Create();
    }
    var repxToJsonConverter = new RepxToJsonConverter(inputPath!, outputPath!);
    foreach (var fileName in repxFiles)
    {
        var relativePath = Path.GetRelativePath(inputPath!, fileName);
        repxToJsonConverter.ConvertToJson(relativePath);
    }
    Console.WriteLine($"Converted {repxFiles.Length} repx files to json");
}
Console.ReadKey(true);