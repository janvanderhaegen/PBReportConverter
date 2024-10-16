using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBReportConverter.Converters;

public static class PblToSrdConverter
{ 
    public static void Unpack(string pebble)
    {
        Console.WriteLine($"Unpacking {pebble}...");            
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "pbdumper";
        process.StartInfo.Arguments = $"-f \"{pebble}\"";
        process.Start();
        process.WaitForExit(20000);
        if (process.ExitCode != 0)
            throw new Exception($"cmd pbdumper -f \"{pebble}\" didn't properly exit");
    }
}
